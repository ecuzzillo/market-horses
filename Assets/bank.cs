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
    public static bank Instance;
    private bool addedInfo;

    public bank()
    {
        Instance = this;
        addedInfo = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        //networkManager.autoCreatePlayer = false;
        /*networkManager.playerPrefab = playerPrefab;
        ClientScene.RegisterPrefab(playerPrefab);
        networkManager.spawnPrefabs.Add(playerPrefab);*/
        counter = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if (!addedInfo && ClientScene.localPlayers.Count > 0)
        {
            if (!isServer)
            {
            }
            else
            {
                Debug.Log($"adding player info for server");
                AddPlayerInfo(ClientScene.localPlayers[0].gameObject.GetComponent<NetworkIdentity>());
            }
            addedInfo = true;
        }
    }


    public void AddPlayerInfo(NetworkIdentity id)
    {
        foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
        {
            goods.Add(new BankGoodInfo
            {
                type = mygoodtype,
                inventory = 100,
                price = 10.0f,
                futuresPrice = 2.0f,
                playerPositions = new PlayerGoodInfo[] {
                        new PlayerGoodInfo {
                            id = id,
                            futurePositions = new FuturePosition[] { },
                            position = 0
                        }
                    }
            });
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
