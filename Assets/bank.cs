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

public struct BankGoodInfo
{
    GoodType type;
    int inventory;
    float price;
}



public class bank : NetworkBehaviour
{
    [SyncVar]
    public int health;

    //SyncListStruct<BankGoodInfo> goods;
    public int counter;
    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
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
}
