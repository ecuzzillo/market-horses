using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public NetworkInstanceId id;
    public NetworkIdentity idobj;
    bool added = false;

    // Start is called before the first frame update
    void Start()
    {
        idobj = GetComponent<NetworkIdentity>();
        id = idobj.netId;
    }

    // Update is called once per frame
    void Update()
    {
        if (!added && ClientScene.localPlayers.Count > 0)
        {
            added = true;
            if (idobj.hasAuthority)
            {
                Debug.Log("trying to add player info for " + idobj.isLocalPlayer);
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
    public void CmdBuyStock(GoodType type, NetworkInstanceId id, int inc)
    {
        bank.Instance.BuyStock(type, id, inc);
    }
}
