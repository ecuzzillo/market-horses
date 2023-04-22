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
    Pigs,
    Sheep,
    Bricks,
    Lightbulbs,
    Watches
}


public struct BankGoodInfo : INetworkSerializeByMemcpy, IEquatable<BankGoodInfo>
{
    public GoodType type;
    public int inventory;
    public float price;
    public float futuresPrice;
    //public PlayerGoodInfo playerPositions;
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

    /*public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {*/
        /*serializer.SerializeValue(ref type);
        serializer.SerializeValue(ref inventory);*/
        //serializer.SerializeValue(ref price);
        //serializer.SerializeValue(ref futuresPrice);
        /*var forceNetworkSerializeByMemcpy = new ForceNetworkSerializeByMemcpy<FixedList4096Bytes<PlayerGoodInfo>>(playerPositions);
        serializer.SerializeValue(ref forceNetworkSerializeByMemcpy);*/
    //}
}

public class bank : NetworkBehaviour
{
    public NetworkVariable<int> health;

    public NetworkList<BankGoodInfo> goods;
    public int counter;
    public NetworkManager networkManager;
    /*public NetworkInstanceId myID;*/
    public GameObject playerPrefab;
    public static bank Instance;

    public bank()
    {
        Instance = this;
    }

    void Start()
    {
        goods = new NetworkList<BankGoodInfo>();
        //networkManager.au = false;
        /*networkManager.playerPrefab = playerPrefab;
        ClientScene.RegisterPrefab(playerPrefab);
        networkManager.spawnPrefabs.Add(playerPrefab);*/
        //myID = GetComponent<NetworkObject>().netId;
        counter = 0;

    }

    public void AddPlayerInfo(ulong id)
    {
        if (goods.Count > 0)
        {
            //this may or may not make sense anymore
            /*for (int i = 0; i < goods.Count; i++)
            {
                var good = goods[i];
                good.playerPositions = good.playerPositions.Append(default(PlayerGoodInfo)).ToArray();
                goods[i] = good;
            }*/
        }
        else
        {
            foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
            {
                goods.Add(new BankGoodInfo()
                /*{
                    type = mygoodtype,
                    inventory = 100,
                    price = 12.0f,
                    futuresPrice = 2.0f,
                    playerPositions = default/*new[] {
                        new PlayerGoodInfo {
                            //id = id,
                            //futurePositions = new FuturePosition[] { },
                            position = 0
                        }
                    }* /
                }*/);
            }
        }
    }

    [ServerRpc]
    void CmdBuyGoodServerRpc(GoodType type, ulong id, int inc)
    {
        if (IsServer)
        {
            for (int i = 0; i < goods.Count; i++)
            {
                var good = goods[i];
                /*if (good.type == type)
                {
                   good.inventory += inc;
                }*/
                goods[i] = good;
                break;
            }
        }
    }

    public void BuyStock(GoodType type, ulong id, int inc)
    {
        for (int i=0; i<goods.Count; i++)
        {
            var good = goods[i];
            /*if (good.type == type)
            {
                var positions = good.playerPositions;
                /*for (int j=0; j<positions.Length; j++)
                {
                    var pos = positions[j];
                    if (pos.id == id)
                    {
                        // i have you now
                        pos.position += inc;
                        positions[j] = pos;
                        good.inventory -= inc;
                        goods[i] = good;

                        return;
                    }
                }* /
            }*/
        }
    }
}
