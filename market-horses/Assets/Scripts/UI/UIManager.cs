﻿using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;


public struct VisualEventInfo
{
    public GoodType type;
    public EventInfo info;
    public float ReceivedTime;
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    public UIDocument document;
    public NetworkManager networkManager;
    public string localPlayerName;
    public VisualElement tickerSection;
    public VisualElement managersSection;
    public VisualElement goodsSection;
    public VisualElement tradeSection;
    public VisualElement gameScreen;

    public VisualElement startScreen;

    public GoodType goodToTrade;

    
    public ListView tickerView;
    public List<VisualEventInfo> events;

    public MultiColumnListView playerListView;
    public MultiColumnListView mclv;
    

    public void Start()
    {
        Instance = this;

        events = new List<VisualEventInfo>();

        document = GetComponent<UIDocument>();

        tickerSection = document.rootVisualElement.Q("ticker-section");
        managersSection = document.rootVisualElement.Q("managers-section");
        goodsSection = document.rootVisualElement.Q("goods-section");
        tradeSection = document.rootVisualElement.Q("trade-section");
        startScreen = document.rootVisualElement.Q("start-screen");
        gameScreen = document.rootVisualElement.Q("game-screen");

        /*var asset = Resources.Load<VisualTreeAsset>("Main");
        asset.CloneTree(document.rootVisualElement);*/

        startScreen.style.display = DisplayStyle.Flex;
        startScreen.Q<Button>("join-game-button").RegisterCallback<ClickEvent>(OnJoinGameClick);
        startScreen.Q<Button>("host-game-button").RegisterCallback<ClickEvent>(OnHostGameClick);
        
        tradeSection.style.display = DisplayStyle.None;
        tradeSection.Q<Button>("exit-button").RegisterCallback<ClickEvent>(OnTradeExitClick);
        tradeSection.Q<Button>("buy-button").RegisterCallback<ClickEvent>(OnTradeBuyClick);
        tradeSection.Q<Button>("sell-button").RegisterCallback<ClickEvent>(OnTradeSellClick);

        tickerView = document.rootVisualElement.Q<ListView>("ticker-list");
        tickerView.style.display = DisplayStyle.None;
        tickerView.makeItem = () => new Label();
        tickerView.bindItem = (element, i) =>
        {
            var myevent = events[i];
            Debug.Log($"you bet your bottom krone {element} {i}");
            (element as Label).text = $"{myevent.ReceivedTime} at time {myevent.info.secondsFromStart}, {myevent.info.quantity} units of {myevent.type} will be transacted!";
        };
        tickerView.itemsSource = events;

        playerListView = document.rootVisualElement.Q<MultiColumnListView>("player-list");
        playerListView.style.display = DisplayStyle.None;
        playerListView.columns.stretchMode = Columns.StretchMode.GrowAndFill;
        playerListView.columns["name"].makeCell = () => new Label();
        playerListView.columns["networth"].makeCell = () => new Label();
        
        playerListView.columns["name"].bindCell = (element, i) =>
        {
            (element as Label).text = "howdy fix me";
        };
        playerListView.columns["networth"].bindCell = (element, i) => new Label();


        mclv = document.rootVisualElement.Q<MultiColumnListView>("mclv");
        mclv.style.display = DisplayStyle.None;

        mclv.columns.stretchMode = Columns.StretchMode.GrowAndFill;
        mclv.columns["name"].makeCell = () => new Label();
        mclv.columns["price"].makeCell = () => new Label();
        mclv.columns["position"].makeCell = () => new Label();
        mclv.columns["supply"].makeCell = () => new Label();
        mclv.columns["trade"].makeCell = () => new Button();
                
        mclv.columns["name"].bindCell = (element, index) =>
            (element as Label).text = ((GoodType)index).ToString();
        mclv.columns["price"].bindCell = (element, index) =>
            (element as Label).text = bank.Instance.goods[index].price.ToString();
        mclv.columns["position"].bindCell = (element, index) =>
        {
            var bgi = bank.Instance.goods[index];

            for (int i = 0; i < bgi.playerPositions.Length; i++)
            {
                if (bgi.playerPositions[i].id == Player.LocalPlayerId())
                {
                    (element as Label).text = bgi.playerPositions[i].position.ToString();
                    return;
                }
            }
        };
        mclv.columns["supply"].bindCell = (element, index) =>
        {
            (element as Label).text = bank.Instance.goods[index].inventory.ToString();
        };
        mclv.columns["trade"].bindCell = (element, index) =>
        {
            (element as Button).text = "Trade";
            (element as Button).clicked -=
                () => ShowTradeView((GoodType)index);
            (element as Button).clicked +=
                () => ShowTradeView((GoodType)index);
        };
        
        
    }

    [ClientRpc]
    public void GetEventPingClientRpc(ulong playerId, GoodType goodType, EventInfo e)
    {
        var localPlayerId = Player.LocalPlayerId();
        if (playerId == localPlayerId)
        {
            Debug.Log($"added event with receivedtime {Time.time} to list!");
            events.Add(new VisualEventInfo { info = e, ReceivedTime = Time.time, type = goodType });
            tickerView.RefreshItems();
        }
        else
        {
            Debug.Log($"wanted id {playerId} but got {localPlayerId}");
        }
    }

    private void Update()
    {

        if (bank.Instance.IsSpawned && bank.Instance.goods.Count > 0)
        {
            var shitlist = new List<BankGoodInfo>();
                
            for (int i = 0; i < (int)GoodType.NumGoodType; i++)
            {
                shitlist.Add(bank.Instance.goods[i]);
            }

            mclv.itemsSource = shitlist;
            mclv.RefreshItems();

            document.rootVisualElement.Q<Label>("trade-price-label").text =
                bank.Instance.goods[(int)goodToTrade].price.ToString();
            tradeSection.Q<Label>("trade-price-label").text =
                bank.Instance.goods[(int)goodToTrade].price.ToString();
            tradeSection.Q<Label>("trade-position").text =
                bank.Instance.GetPlayerGoodInfo(goodToTrade, Player.LocalPlayerId()).position.ToString();
        }
    }

    public void ShowTradeView(GoodType goodType)
    {
        goodToTrade = goodType;
        managersSection.style.display = DisplayStyle.None;
        goodsSection.style.display = DisplayStyle.None;
        tradeSection.style.display = DisplayStyle.Flex;
    }

    public void OnJoinGameClick(ClickEvent evt)
    {
        //slurp ip to connect to from text box
        //slurp name from other text box
        //call addplayerinfo
        var ip = startScreen.Q<TextField>("ip-address-field").value;
        localPlayerName = startScreen.Q<TextField>("name-field").value;

        networkManager.GetComponent<UnityTransport>().ConnectionData.Address = ip;
        networkManager.StartClient();

        ExitStartScreen();

    }
    
    public void OnHostGameClick(ClickEvent evt)
    {
        //slurp ip to connect to from text box
        //slurp name from other text box
        //call addplayerinfo
        localPlayerName = startScreen.Q<TextField>("name-field").value;

        networkManager.StartHost();

        ExitStartScreen();

    }

    private void ExitStartScreen()
    {
        startScreen.style.display = DisplayStyle.None;
        tickerView.style.display = DisplayStyle.Flex;
        managersSection.style.display = DisplayStyle.Flex;
        goodsSection.style.display = DisplayStyle.Flex;
        gameScreen.style.display = DisplayStyle.Flex;
    }

    public void OnTradeBuyClick(ClickEvent evt)
    {
        Player.LocalPlayer().CmdBuyStockServerRpc(goodToTrade, 1);
    }

    public void OnTradeSellClick(ClickEvent evt)
    {
        bank.Instance.SellStockServerRpc(goodToTrade, Player.LocalPlayerId(), 1);
    }

    public void OnTradeExitClick(ClickEvent evt)
    {
        ShowMainView();
    }

    public void ShowMainView()
    {
        Debug.Log("wtf 4");
        managersSection.style.display = DisplayStyle.Flex;
        goodsSection.style.display = DisplayStyle.Flex;
        tradeSection.style.display = DisplayStyle.None;
        mclv.RefreshItems();
    }
}