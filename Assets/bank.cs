using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class SyncListBankGoodInfo : SyncListStruct<BankGoodInfo> { }

public struct BankGoodInfo
{
    public GoodType type;
    public int inventory;
    public float price;
    public float futuresPrice;
    public PlayerGoodInfo[] playerPositions;
}

public class bank : NetworkBehaviour
{
    [SyncVar]
    public int health;

    public SyncListBankGoodInfo goods = new SyncListBankGoodInfo();
    public int counter;
    public NetworkManager networkManager;
    public NetworkInstanceId myID;
    public GameObject playerPrefab;
    public static bank Instance;

    public bank()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //networkManager.autoCreatePlayer = false;
        /*networkManager.playerPrefab = playerPrefab;
        ClientScene.RegisterPrefab(playerPrefab);
        networkManager.spawnPrefabs.Add(playerPrefab);*/
        myID = GetComponent<NetworkIdentity>().netId;
        counter = 0;

    }

    public void AddPlayerInfo(NetworkInstanceId id)
    {
        if (goods.Count > 0)
        {
            for (int i = 0; i < goods.Count; i++)
            {
                var good = goods[i];
                good.playerPositions = good.playerPositions.Append(new PlayerGoodInfo
                {
                    id = id,
                    futurePositions = new FuturePosition[] { },
                    position = 0
                }).ToArray();
                goods[i] = good;
            }
        }
        else
        {
            foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
            {
                goods.Add(new BankGoodInfo
                {
                    type = mygoodtype,
                    inventory = 100,
                    price = 12.0f,
                    futuresPrice = 2.0f,
                    playerPositions = new[] {
                        new PlayerGoodInfo {
                            id = id,
                            futurePositions = new FuturePosition[] { },
                            position = 0
                        }
                    }
                });
            }
        }
    }

    [Command]
    void CmdBuyGood(GoodType type, NetworkIdentity id, int inc)
    {
        if (isServer)
        {
            for (int i = 0; i < goods.Count; i++)
            {
                var good = goods[i];
                if (good.type == type)
                {
                   good.inventory += inc;
                }
                goods[i] = good;
                break;
            }
        }
    }

    public void BuyStock(GoodType type, NetworkInstanceId id, int inc)
    {
        for (int i=0; i<goods.Count; i++)
        {
            var good = goods[i];
            if (good.type == type)
            {
                var positions = good.playerPositions;
                for (int j=0; j<positions.Length; j++)
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
                }
            }
        }
    }
}
