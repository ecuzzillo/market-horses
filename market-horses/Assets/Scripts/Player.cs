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

    public static Player LocalPlayer()
    {
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
                return player;
        }

        throw new Exception("aw geez no player");
    }

    // Start is called before the first frame update
    void Start()
    {
        idobj = GetComponent<NetworkObject>();
        id = idobj.NetworkObjectId;
    }

    // Update is called once per frame
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

        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K");
            CmdBuyStockServerRpc(GoodType.Cotton, 1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("J");
            var bank = FindAnyObjectByType<bank>();
            Debug.Log("Position: " + bank.goods[0].playerPositions[0].position);
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
        FindAnyObjectByType<bank>().AddPlayerInfo(id, "player name here");
    }

    [ServerRpc]
    public void CmdBuyStockServerRpc(GoodType type, int inc)
    {
        bank.Instance.BuyStockServerRpc(type, id, inc);
    }
}
