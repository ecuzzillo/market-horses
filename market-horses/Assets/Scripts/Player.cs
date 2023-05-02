using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

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

    public static int PlayerIdxFromId(ulong id)
    {
        return bank.Instance.playerIds.IndexOf(id);
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
            CmdAddPlayerInfoServerRpc(UIManager.Instance.localPlayerName);
            UIManager.Instance.UpdateForNewPlayer();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Spawned player {id}");
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    void CmdAddPlayerInfoServerRpc(string playerName)
    {
        Debug.Log("trying to add player info for " + id);
        FindAnyObjectByType<bank>().AddPlayerInfo(id, playerName);
    }
    
    // this has to be on the player because the client has to own the object it calls serverrpc's on    
    [ServerRpc]
    public void AddNewOfferServerRpc(Offer newoffer)
    {
        bank.Instance.allOffers.Add(newoffer);
    }

    [ServerRpc]
    public void CmdBuyStockServerRpc(GoodType type, int inc)
    {
        bank.Instance.BuyStockServerRpc(type, id, inc);
    }
    
    [ClientRpc]
    public void StartGameOnClientRpc()
    {
        UIManager.Instance.startGameButton.style.display = DisplayStyle.None;
        UIManager.Instance.tickerSection.style.display = DisplayStyle.Flex;
    }
}
