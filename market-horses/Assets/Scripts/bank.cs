using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public enum GoodType
{
    Horses,
    Cotton,
    Oil,
    Cows,
    NumGoodType
}

public struct EventInfo
{
    public int quantity;
    public int secondsFromStart;
    public bool done;
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

public struct PlayerEventNotificationInfo
{
    public int notificationLeadTime;
    public GoodType goodType;
    public int eventIdx;
}

public class bank : NetworkBehaviour
{
    public NetworkVariable<int> health;
    public int PriceChangeAmount;
    public float PlayerStartingCash;

    public NetworkList<BankGoodInfo> goods;
    public NetworkList<FixedString128Bytes> playerNames;
    public NetworkList<ulong> playerIds;
    public NetworkList<int> playerFreeCash;
    public int counter;
    public NetworkManager networkManager;
    public ulong myID;
    public GameObject playerPrefab;
    public static bank Instance;

    public float gameStart;
    public float probabilityOfTellingPlayerAboutAGivenEvent;
    public Dictionary<ulong, List<PlayerEventNotificationInfo>> pings =
        new Dictionary<ulong, List<PlayerEventNotificationInfo>>();

    public bank()
    {
        gameStart = -1;
        Instance = this;
    }

    void Start()
    {
        goods = new NetworkList<BankGoodInfo>();
        playerNames = new NetworkList<FixedString128Bytes>();
        playerIds = new NetworkList<ulong>();
        playerFreeCash = new NetworkList<int>();
        
        myID = GetComponent<NetworkObject>().NetworkObjectId;
        counter = 0;
    }

    public void AddPlayerInfo(ulong id, string playername)
    {
        Debug.Log($"Server: {IsServer} -- Host: {IsHost} -- Client: {IsClient}");
        if (!IsHost) { Debug.Log("WE NOT SERVER NOW");  return; }
        Debug.Log("WE SERVER NOW");
        if (goods.Count > 0)
        {
            for (int i = 0; i < goods.Count; i++)
            {
                var good = goods[i];
                good.playerPositions.Add(new PlayerGoodInfo()
                {
                    id = id,
                    position = 0
                });
                goods[i] = good;
            }
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
                bgi.playerPositions.Add(new PlayerGoodInfo()
                {
                    id = id,
                    position = 0
                });
                goods.Add(bgi);
            }
            UIManager.Instance.mclv.RefreshItems();

            for (int i = 0; i < 20; i++)
            {
                var myevent = new EventInfo();
                var type = UnityEngine.Random.Range(0, ((int)GoodType.NumGoodType - 1));
                myevent.quantity = Random.Range(-100, 100);
                myevent.secondsFromStart = Random.Range(0, 120);
                var tmp = goods[type];
                tmp.eventsForThisGood.Add(myevent);
                goods[type] = tmp;
            }
        }
        
        playerNames.Add(playername);
        playerIds.Add(id);
        playerFreeCash.Add((int)PlayerStartingCash);
    }

    private void Update()
    {
        if (goods.Count == 0) return;
        
        var time = Time.time;
        for (int i=0; i<(int)GoodType.NumGoodType; i++)
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
                    Debug.Log($"Just handled event {i} {j}");
                }

                events[j] = eventInfo;
            }

            var tmp = goods[i];
            tmp.eventsForThisGood = events;
            goods[i] = tmp;
        }

        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.G) && gameStart < 0)
            {
                Debug.Log("G!");
                gameStart = Time.time;

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
                                Debug.Log($"added thing {i} {j}");
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
            else if (gameStart > 0)
            {
                var timeSinceStartAsOfnow = Time.time - gameStart;
                foreach (var (k, v) in pings)
                {
                    for (int i = v.Count - 1; i >= 0; i--)
                    {
                        var entry = v[i];
                        var eventInfo = goods[(int)entry.goodType].eventsForThisGood[entry.eventIdx];
                        if (eventInfo.secondsFromStart - entry.notificationLeadTime < timeSinceStartAsOfnow)
                        {
                            Debug.Log($"trying to notify??? about event {entry.goodType} {eventInfo.quantity} for player {k}");
                            UIManager.Instance.GetEventPingClientRpc(k, entry.goodType, eventInfo);
                            v.RemoveAt(i);
                        }

                    }
                }
            }
            else
            {
                Debug.Log($"gamestart was {gameStart} so fuck off");
            }

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

        throw new Exception("Player position not found");
    }

    [ServerRpc]
    void CmdBuyGoodServerRpc(GoodType type, ulong id, int inc)
    {
        if (IsServer)
        {
            for (int i = 0; i < goods.Count; i++)
            {
                var good = goods[i];
                if (good.type == type)
                {
                    good.inventory += inc;
                    goods[i] = good;
                    break;
                }
            }
        }
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
                        good.price -= PriceChangeAmount;
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
