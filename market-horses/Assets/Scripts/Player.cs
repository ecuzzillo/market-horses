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
        //bank.Instance.allOffers.Add(newoffer);
        var offeringPlayerIdx = bank.Instance.playerIds.IndexOf(newoffer.OfferingPlayerId);
        var offereePlayerIdx = bank.Instance.playerIds.IndexOf(newoffer.OffereePlayerId);

        if (newoffer.SendingGoods)
        {
            var bgi = bank.Instance.goods[(int)newoffer.goodType];
            var offeringPos = bgi.playerPositions[offeringPlayerIdx];
            offeringPos.position -= newoffer.count;
            bgi.playerPositions[offeringPlayerIdx] = offeringPos;
            var offereePos = bgi.playerPositions[offereePlayerIdx];
            offereePos.position += newoffer.count;
            bgi.playerPositions[offereePlayerIdx] = offereePos;

            bank.Instance.goods[(int)newoffer.goodType] = bgi;
        }
        else
        {
            bank.Instance.playerFreeCash[offeringPlayerIdx] -= newoffer.count;
            bank.Instance.playerFreeCash[offereePlayerIdx] += newoffer.count;
        }
    }

    [ServerRpc]
    public void CmdBuyStockServerRpc(GoodType type, int inc)
    {
        if (inc > 0)
        {
            bank.Instance.BuyStockServerRpc(type, id, inc);
        }
        else
        {
            bank.Instance.SellStockServerRpc(type, id, -inc);
        }
    }
    
    [ClientRpc]
    public void StartGameOnClientRpc()
    {
        UIManager.Instance.startGameButton.style.display = DisplayStyle.None;
        UIManager.Instance.tickerSection.style.display = DisplayStyle.Flex;
    }
    
    [ClientRpc]
    public void GetEventPingClientRpc(ulong playerId, GoodType goodType, EventInfo e)
    {
        var localPlayerId = LocalPlayerId();
        if (playerId == localPlayerId)
        {
            UIManager.Instance.events.Add(new VisualEventInfo { info = e, ReceivedTime = Time.time, type = goodType });
            UIManager.Instance.events.Sort((a,b) => a.info.secondsFromStart.CompareTo(b.info.secondsFromStart));
            UIManager.Instance.tickerView.RefreshItems();
        }
    }

    //has to be on player because local client owns this player object
    [ServerRpc]
    public void ConsummateDealServerRpc(ulong offerGuid)
    {
        bank.Instance.ConsummateDeal(offerGuid);
    }

    [ServerRpc]
    public void RejectDealServerRpc(ulong offerGuid)
    {
        for (int i = 0; i < bank.Instance.allOffers.Count; i++)
        {
            if (bank.Instance.allOffers[i].guid == offerGuid)
            {
                bank.Instance.allOffers.RemoveAt(i);
                return;
            }
        }
    }
    
}
