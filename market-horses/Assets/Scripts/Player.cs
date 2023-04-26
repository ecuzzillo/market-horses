using System;
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

    public static ulong LocalPlayerId()
    {
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
            {
                return player.id;
            }
        }
        Debug.Log("Oh NOOOO");
        return 0;
    }
    
    public static int LocalPlayerIdx()
    {
        return bank.Instance.playerIds.IndexOf(LocalPlayerId());
    }

    public static Player LocalPlayer()
    {
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
                return player;
        }

        throw new Exception("aw geez no player");
    }

    void Start()
    {
        idobj = GetComponent<NetworkObject>();
        id = idobj.NetworkObjectId;
    }

    void Update()
    { 
        if (idobj.IsOwner && !added)
        {
            Debug.Log($"trying to add player info for player {id}");
            var bank = FindAnyObjectByType<bank>();
            var no = bank.GetComponent<NetworkObject>();
            Debug.Log($"IsSpawned: {no.IsSpawned}");
            if (!no.IsSpawned)
            {
                return;
            }
            added = true;
            CmdAddPlayerInfoServerRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Spawned player {id}");
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    void CmdAddPlayerInfoServerRpc()
    {
        Debug.Log("trying to add player info for " + id);
        FindAnyObjectByType<bank>().AddPlayerInfo(id, UIManager.Instance.localPlayerName);
    }

    [ServerRpc]
    public void CmdBuyStockServerRpc(GoodType type, int inc)
    {
        bank.Instance.BuyStockServerRpc(type, id, inc);
    }
}
