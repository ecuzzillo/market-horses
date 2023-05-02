using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

[Serializable]
public struct PlayerGoodInfo : INetworkSerializeByMemcpy
{
    public ulong id;
    public int position;
}

[Serializable]
public struct Offer : INetworkSerializeByMemcpy, IEquatable<Offer>
{
    public bool OfferToBuy;
    public GoodType goodType;
    public ulong OfferingPlayerId;
    public ulong OffereePlayerId;
    public int count;
    public float price;
    public ulong guid;

    public bool Equals(Offer other)
    {
        return OfferToBuy == other.OfferToBuy &&
               goodType == other.goodType &&
               OfferingPlayerId == other.OfferingPlayerId &&
               OffereePlayerId == other.OffereePlayerId &&
               count == other.count &&
               price.Equals(other.price) && 
               guid == other.guid;
    }

    public override bool Equals(object obj)
    {
        return obj is Offer other && Equals(other);
    }

    public override int GetHashCode()
    {
        return guid.GetHashCode();
    }
}

public enum GoodType
{
    Horses,
    Cotton,
    Oil,
    Cows,
    NumGoodType
}

[Serializable]
public struct EventInfo
{
    public int quantity;
    public int secondsFromStart;
    public bool done;
}

[Serializable]
public struct GameStateInfo : INetworkSerializeByMemcpy, IEquatable<GameStateInfo>
{
    public int dayNumber;
    public bool marketOpenNow;
    public float gameStartTime;
    public float secondsUntilMarketToggles;

    public bool Equals(GameStateInfo other)
    {
        return dayNumber == other.dayNumber &&
               marketOpenNow == other.marketOpenNow &&
               gameStartTime.Equals(other.gameStartTime) &&
               secondsUntilMarketToggles.Equals(other.secondsUntilMarketToggles);
    }

    public override bool Equals(object obj)
    {
        return obj is GameStateInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(dayNumber, marketOpenNow, gameStartTime, secondsUntilMarketToggles);
    }
}


public struct BankGoodInfo : INetworkSerializeByMemcpy, IEquatable<BankGoodInfo>
{
    public GoodType type;
    public int inventory;
    public float price;
    public float futuresPrice;
    public FixedList512Bytes<PlayerGoodInfo> playerPositions;
    public FixedList512Bytes<EventInfo> eventsForThisGood;

    public bool Equals(BankGoodInfo other)
    {
        return type == other.type &&
               inventory == other.inventory &&
               price.Equals(other.price) &&
               futuresPrice.Equals(other.futuresPrice) &&
               playerPositions.Equals(other.playerPositions);
    }

    public override bool Equals(object obj)
    {
        return obj is BankGoodInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)type, inventory, price, futuresPrice, playerPositions);
    }
}

[Serializable]
public struct PlayerEventNotificationInfo
{
    public int notificationLeadTime;
    public GoodType goodType;
    public int eventIdx;
}

public class bank : NetworkBehaviour
{
    [Header("Tuning params")]
    public int PriceChangeAmount;
    public float PlayerStartingCash;
    public int PlayerStartingGoodsValue;
    public float probabilityOfTellingPlayerAboutAGivenEvent;
    public float SecondsOfOpenMarketPerDay;
    public float SecondsOfClosedMarketPerNight;
    public float SecondsBetweenClockUIUpdates;
    public int NumberOfDaysInGame;

    [Header("Network stuff")]
    public NetworkList<BankGoodInfo> goods;
    public NetworkList<FixedString128Bytes> playerNames;
    public NetworkList<ulong> playerIds;
    public NetworkList<float> playerFreeCash;
    public NetworkList<Offer> allOffers;
    public NetworkVariable<GameStateInfo> gameState;

    public ulong nextGuid;

    public static bank Instance;

    public Dictionary<ulong, List<PlayerEventNotificationInfo>> pings =
        new Dictionary<ulong, List<PlayerEventNotificationInfo>>();

    public bank()
    {
        Instance = this;
    }

    void Start()
    {
        Screen.SetResolution(390, 844, false);
        goods = new NetworkList<BankGoodInfo>();
        playerNames = new NetworkList<FixedString128Bytes>();
        playerIds = new NetworkList<ulong>();
        playerFreeCash = new NetworkList<float>();
        allOffers = new NetworkList<Offer>();
        gameState = new NetworkVariable<GameStateInfo>();
        gameState.Value = new GameStateInfo()
            { dayNumber = -1, gameStartTime = -1, marketOpenNow = false, secondsUntilMarketToggles = -1 };
    }

    public void AddPlayerInfo(ulong id, string playername)
    {
        Debug.Log($"Server: {IsServer} -- Host: {IsHost} -- Client: {IsClient}");
        if (!IsHost) { Debug.Log("WE NOT SERVER NOW");  return; }
        Debug.Log("WE SERVER NOW");
        playerNames.Add(playername);
        playerIds.Add(id);
        playerFreeCash.Add((int)PlayerStartingCash);
        
        if (goods.Count > 0)
        {
            GeneratePlayerPositionsForId(id);
        }
        else
        {
            for (int i=0; i<(int)GoodType.NumGoodType; i++)
            {
                var goodType = (GoodType)i;
                var bgi = new BankGoodInfo()
                {
                    type = goodType,
                    inventory = 100,
                    price = 12.0f,
                    futuresPrice = 2.0f,
                    playerPositions = default
                };

                goods.Add(bgi);
            }
            GeneratePlayerPositionsForId(id);

            UIManager.Instance.UpdateForNewPlayer();

            for (int i = 0; i < 20; i++)
            {
                var myevent = new EventInfo();
                var type = Random.Range(0, ((int)GoodType.NumGoodType - 1));
                myevent.quantity = Random.Range(-100, 100);
                myevent.secondsFromStart = Random.Range(0, 120);
                var tmp = goods[type];
                tmp.eventsForThisGood.Add(myevent);
                goods[type] = tmp;
            }
        }
    }
    

    private void GeneratePlayerPositionsForId(ulong id)
    {
        var positions = new float[goods.Count];
        var sum = 0f;
        for (int i = 0; i < goods.Count; i++)
        {
            var position = Random.Range(0f, 1f);
            positions[i] = position;
            sum += position;
        }

        for (int i = 0; i < goods.Count; i++)
        {
            positions[i] = (positions[i] / sum) * PlayerStartingGoodsValue / goods[i].price;
        }
        
        for (int i = 0; i < goods.Count; i++)
        {
            var good = goods[i];
            good.playerPositions.Add(new PlayerGoodInfo()
            {
                id = id,
                position = (int)positions[i]
            });
            goods[i] = good;
        }
    }

    [ServerRpc]
    public void ConsummateDealServerRpc(ulong offerGuid)
    {
        //figure out how much good to transfer
        //figure out how much money to transfer
        //remove offer from alloffers list
        //call ui update
        Offer o = default;
        o.guid = ulong.MaxValue;
        for (int i = 0; i < allOffers.Count; i++)
        {
            if (allOffers[i].guid == offerGuid)
            {
                o = allOffers[i];
                allOffers.RemoveAt(i);
                break;
            }
        }

        if (o.guid == ulong.MaxValue)
        {
            //this actually happens a fair bit, and i guess it's not a problem because our guid scheme saved us
            //Debug.LogError("OH NO WE'VE BEEN FUCKED");
            return;
        }
        
        var offereeIdx = Player.PlayerIdxFromId(o.OffereePlayerId);
        var offeringIdx = Player.PlayerIdxFromId(o.OfferingPlayerId);

        var receivingGoodsIdx = o.OfferToBuy ? offeringIdx : offereeIdx;
        var receivingCashIdx = o.OfferToBuy ? offereeIdx : offeringIdx;
        for (int i = 0; i < goods.Count; i++)
        {
            if (i == (int)o.goodType)
            {
                var overallCashToTransfer = o.count * o.price;
                var tmpbgi = goods[i];
                var tmppgiReceivingCash = tmpbgi.playerPositions[receivingCashIdx];
                if (tmppgiReceivingCash.position < o.count)
                {
                    Debug.LogError($"OH NO! Trying to consummate offer we're not supposed to! Player " +
                                   $"{receivingCashIdx} didn't have enough of {o.goodType} to do the deal with " +
                                   $"{receivingGoodsIdx}, but we tried anyway!");
                }
                tmppgiReceivingCash.position -= o.count;
                playerFreeCash[receivingCashIdx] += overallCashToTransfer;
                tmpbgi.playerPositions[receivingCashIdx] = tmppgiReceivingCash;

                var tmppgiReceivingGoods = tmpbgi.playerPositions[receivingGoodsIdx];
                tmppgiReceivingGoods.position += o.count;
                if (playerFreeCash[receivingGoodsIdx] < overallCashToTransfer)
                {
                    Debug.LogError($"OH NO! Trying to consummate offer we're not supposed to! Player" +
                                   $" {receivingGoodsIdx} didn't have enough of cash to do the deal with " +
                                   $"{receivingCashIdx}, but we tried anyway!");
                }
                playerFreeCash[receivingGoodsIdx] -= overallCashToTransfer;
                tmpbgi.playerPositions[receivingGoodsIdx] = tmppgiReceivingGoods;

                goods[i] = tmpbgi;
                return;
            }
        }
        Debug.LogError("OH NO BAD GOOD");
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (goods.Count == 0) return;

        ProcessEvents();

        var gameStateV = gameState.Value;
        if (gameStateV.gameStartTime > 0)
        {
            var timeSinceStartAsOfnow = Time.time - gameStateV.gameStartTime;
            var dayLength = (SecondsOfOpenMarketPerDay + SecondsOfClosedMarketPerNight);
            var timeInDays = timeSinceStartAsOfnow / dayLength;
            gameStateV.dayNumber = (int)timeInDays;
            var secondsInDaySoFar = timeSinceStartAsOfnow - dayLength * gameStateV.dayNumber;

            gameStateV.marketOpenNow = secondsInDaySoFar < SecondsOfOpenMarketPerDay;

            gameStateV.secondsUntilMarketToggles = gameStateV.marketOpenNow
                ? (SecondsOfOpenMarketPerDay - secondsInDaySoFar)
                : (dayLength - secondsInDaySoFar);

            gameState.Value = gameStateV;

            foreach (var (k, v) in pings)
            {
                for (int i = v.Count - 1; i >= 0; i--)
                {
                    var entry = v[i];
                    var eventInfo = goods[(int)entry.goodType].eventsForThisGood[entry.eventIdx];
                    if (eventInfo.secondsFromStart - entry.notificationLeadTime < timeSinceStartAsOfnow)
                    {
                        UIManager.Instance.GetEventPingClientRpc(k, entry.goodType, eventInfo);
                        v.RemoveAt(i);
                    }

                }
            }
        }
    }

    [ServerRpc]
    public void GenerateEventsForGameServerRpc()
    {
        gameState.Value = new GameStateInfo()
        {
            dayNumber = 0,
            gameStartTime = Time.time,
            marketOpenNow = true,
            secondsUntilMarketToggles = 0
        };
        var playerids = new List<ulong>();

        for (int i = 0; i < goods[0].playerPositions.Length; i++)
        {
            playerids.Add(goods[0].playerPositions[i].id);
        }

        foreach (var id in playerids)
        {
            pings[id] = new List<PlayerEventNotificationInfo>();
        }

        for (int i = 0; i < (int)GoodType.NumGoodType; i++)
        {
            var events = goods[i].eventsForThisGood;
            for (int j = 0; j < events.Length; j++)
            {
                foreach (var id in playerids)
                {
                    var thing = Random.Range(0f, 1f);
                    if (thing < probabilityOfTellingPlayerAboutAGivenEvent)
                    {
                        pings[id]
                            .Add(new PlayerEventNotificationInfo()
                            {
                                eventIdx = j,
                                goodType = (GoodType)i,
                                notificationLeadTime = Random.Range(5, 60)
                            });
                    }
                }
            }
        }
    }


    private void ProcessEvents()
    {
        if (gameState.Value.gameStartTime < 0) return;
        
        var time = Time.time;
        for (int i = 0; i < (int)GoodType.NumGoodType; i++)
        {
            var events = goods[i].eventsForThisGood;
            for (int j = 0; j < events.Length; j++)
            {
                var eventInfo = events[j];
                if (!eventInfo.done && eventInfo.secondsFromStart < time)
                {
                    //do the thing
                    var bankGoodInfo = goods[i];

                    var newInventory = bankGoodInfo.inventory + eventInfo.quantity;
                    if (newInventory < 0)
                    {
                        bankGoodInfo.price += bankGoodInfo.inventory;
                        bankGoodInfo.inventory = 0;
                    }
                    else
                    {
                        bankGoodInfo.inventory = newInventory;
                        //1 is not correct, it should be asymptotic something mumble
                        bankGoodInfo.price = Mathf.Max(1, bankGoodInfo.price - eventInfo.quantity);
                    }

                    goods[i] = bankGoodInfo;
                    eventInfo.done = true;
                }

                events[j] = eventInfo;
            }

            var tmp = goods[i];
            tmp.eventsForThisGood = events;
            goods[i] = tmp;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("I'm A BANK");
    }

    public PlayerGoodInfo GetPlayerGoodInfo(GoodType goodType, ulong playerId)
    {
        var good = goods[(int)goodType];
        for (int i = 0; i < good.playerPositions.Length; i++)
        {
            if (good.playerPositions[i].id == playerId)
            {
                return good.playerPositions[i];
            }
        }

        throw new Exception($"Player position not found for playerid {playerId}");
    }

    [ServerRpc]
    public void BuyStockServerRpc(GoodType type, ulong id, int inc)
    {
        if (!IsServer)
        {
            return;
        }

        Debug.Log($"Trying to buy {inc} stock of {type}");
        var i = (int)type;
        var good = goods[i];
        var positions = good.playerPositions;
        var playerIdx = playerIds.IndexOf(id);
        for (int j = 0; j < positions.Length; j++)
        {
            var pos = positions[j];
            if (pos.id == id)
            {
                for (int k = 0; k < inc; k++)
                {
                    if (good.inventory > 0 && playerFreeCash[playerIdx] >= good.price)
                    {
                        pos.position++;
                        positions[j] = pos;
                        good.inventory--;
                        playerFreeCash[playerIdx] -= (int)good.price;
                        good.price += PriceChangeAmount;
                        good.playerPositions = positions;
                        goods[i] = good;
                    }
                    else
                        break;
                }
                return;
            }
        }
    }

    [ServerRpc]
    public void SellStockServerRpc(GoodType type, ulong id, int inc)
    {
        if (!IsServer)
        {
            return;
        }

        Debug.Log($"Selling {inc} stock of {type}");
        var i = (int)type;

        var good = goods[i];
        var playerIdx = playerIds.IndexOf(id);

        var positions = good.playerPositions;
        for (int j = 0; j < positions.Length; j++)
        {
            var pos = positions[j];
            if (pos.id == id)
            {
                for (int k = 0; k < inc; k++)
                {
                    if (pos.position > 0)
                    {
                        pos.position--;
                        positions[j] = pos;
                        good.inventory++;
                        playerFreeCash[playerIdx] += (int)good.price;
                        if (good.price - PriceChangeAmount > 1) {
                            good.price -= PriceChangeAmount;
                        } else
                        {
                            good.price = 1;
                        }
                        good.playerPositions = positions;
                        goods[i] = good;
                    }
                    else
                        return;
                }

                return;
            }
        }
    }
    
}
