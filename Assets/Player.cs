using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public NetworkIdentity id;
    bool added = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!added && ClientScene.localPlayers.Count > 0)
        {
            added = true;
            id = GetComponent<NetworkIdentity>();
            if (id.hasAuthority)
            {
                Debug.Log("trying to add player info for " + id.isLocalPlayer);
                CmdAddPlayerInfo();
            }
        }
    }

    [Command]
    void CmdAddPlayerInfo()
    {
        Debug.Log("trying to add player info for " + id);
        bank.Instance.AddPlayerInfo(id);
    }

    [Command]
    public void CmdBuyStock(GoodType type, NetworkIdentity id, int inc)
    {
        bank.Instance.BuyStock(type, id, inc);
    }
}
