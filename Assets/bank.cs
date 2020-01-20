using System;
using System.Collections;
using System.Collections.Generic;
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
    public NetworkIdentity myID;
    public GameObject playerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        //networkManager.autoCreatePlayer = false;
        /*networkManager.playerPrefab = playerPrefab;
        ClientScene.RegisterPrefab(playerPrefab);
        networkManager.spawnPrefabs.Add(playerPrefab);*/
        counter = 0;
        if (!isServer)
        {

        }
        else
        {
            foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
            {
                goods.Add(new BankGoodInfo
                {
                    type = mygoodtype,
                    inventory = 100,
                    price = 10.0f,
                    futuresPrice = 2.0f,
                    playerPositions = new PlayerGoodInfo[] { }
                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((counter++ % 15) == 0)
        {
            if (isServer)
            {
                health++;
                Debug.Log($"incrementing health to {health}");
            }
            else
            {
                Debug.Log($"received health is {health}");
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
}
