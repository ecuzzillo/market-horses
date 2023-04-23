using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public enum GoodType
{
    Cotton,
    Oil,
    Silk,
    Cows
}


public struct BankGoodInfo : INetworkSerializeByMemcpy, IEquatable<BankGoodInfo>
{
    public GoodType type;
    public int inventory;
    public float price;
    public float futuresPrice;
    public FixedList4096Bytes<PlayerGoodInfo> playerPositions;

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

public class bank : NetworkBehaviour
{
    public NetworkVariable<int> health;
    public int PriceChangeAmount = 1;

    public NetworkList<BankGoodInfo> goods;
    public int counter;
    public NetworkManager networkManager;
    public ulong myID;
    public GameObject playerPrefab;
    public static bank Instance;

    public bank()
    {
        Instance = this;
    }

    void Start()
    {
        goods = new NetworkList<BankGoodInfo>();
        myID = GetComponent<NetworkObject>().NetworkObjectId;
        counter = 0;
    }

    public void AddPlayerInfo(ulong id)
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
            foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
            {
                var bgi = new BankGoodInfo()
                {
                    type = mygoodtype,
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
                Debug.Log($"Adding mygoodtype {mygoodtype}");
                goods.Add(bgi);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("I'm A BANK");
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
        if (!IsServer) { return; }

        Debug.Log($"Buying {inc} stock of {type}");
        for (int i=0; i<goods.Count; i++)
        {
            var good = goods[i];
            if (good.type == type)
            {
                var positions = good.playerPositions;
                for (int j = 0; j < positions.Length; j++)
                {
                    var pos = positions[j];
                    if (pos.id == id)
                    {
                        for (int k = 0; k < inc; k++)
                        {
                            if (good.inventory > 0)
                            {
                                pos.position++;
                                positions[j] = pos;
                                good.inventory--;
                                good.price += PriceChangeAmount;
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
    }public void SellStockServerRpc(GoodType type, ulong id, int inc)
    {
        if (!IsServer) { return; }

        Debug.Log($"Selling {inc} stock of {type}");
        for (int i=0; i<goods.Count; i++)
        {
            var good = goods[i];
            if (good.type == type)
            {
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
    }
}
