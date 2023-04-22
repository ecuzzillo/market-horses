using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public ulong id;
    public NetworkObject idobj;
    bool added = false;

    // Start is called before the first frame update
    void Start()
    {
        idobj = GetComponent<NetworkObject>();
        id = idobj.NetworkObjectId;
    }

    // Update is called once per frame
    void Update()
    {
        if (!added)// && ClientScene.localPlayers.Count > 0)
        {
            added = true;
            if (idobj.IsOwner)
            {
                Debug.Log("trying to add player info for " + idobj.IsLocalPlayer);
                CmdAddPlayerInfoServerRpc();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("i spawned");
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    void CmdAddPlayerInfoServerRpc()
    {
        Debug.Log("trying to add player info for " + id);
        bank.Instance.AddPlayerInfo(id);
    }

    [ServerRpc]
    public void CmdBuyStockServerRpc(GoodType type, ulong id, int inc)
    {
        bank.Instance.BuyStock(type, id, inc);
    }
}
