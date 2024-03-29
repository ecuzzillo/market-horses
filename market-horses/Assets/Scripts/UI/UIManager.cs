﻿using System;
using System.Collections.Generic;
using System.Linq;
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

    public float dayPercentageLastUpdated;

    public bool localMarketOpenNow;
    
    public UIDocument document;
    public NetworkManager networkManager;
    public string localPlayerName;
    public Label clockLabel;
    public Label localPlayerFreeCashLabel;
    public Label localPlayerNetWorthLabel;
    public VisualElement tickerSection;
    public Button startGameButton;
    public VisualElement managersSection;
    public VisualElement goodsSection;
    public VisualElement tradeSection;
    public VisualElement gameScreen;
    public VisualElement gameEndModal;
    
    public TradeWithPlayerScreen twps;

    public VisualElement startScreen;

    public GoodType goodToTrade;
    
    public ListView tickerView;
    public List<VisualEventInfo> events;

    public MultiColumnListView playerListView;
    public MultiColumnListView mclv;

    public Dictionary<Button, int> tradeButtonShit = new Dictionary<Button, int>();

    public float myLocalGameStartTime;
        

    public void Start()
    {
        Instance = this;
        myLocalGameStartTime = -1;

        events = new List<VisualEventInfo>();
        dayPercentageLastUpdated = -5;

        document = GetComponent<UIDocument>();

        clockLabel = document.rootVisualElement.Q<Label>("clock");
        localPlayerFreeCashLabel = document.rootVisualElement.Q<Label>("self-freecash");
        localPlayerNetWorthLabel = document.rootVisualElement.Q<Label>("self-networth");

        startGameButton = document.rootVisualElement.Q<Button>("start-game-button");
        startGameButton.style.display = DisplayStyle.None;
        startGameButton.RegisterCallback<ClickEvent>(OnStartGameClick);
        
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

        document.rootVisualElement.Q("game-screen").style.display = DisplayStyle.None;
            
        tradeSection.style.display = DisplayStyle.None;
        tradeSection.Q<Button>("exit-button").RegisterCallback<ClickEvent>(OnTradeExitClick);
        tradeSection.Q<Button>("buy-button").RegisterCallback<ClickEvent>(OnTradeBuyClick);
        tradeSection.Q<Button>("sell-button").RegisterCallback<ClickEvent>(OnTradeSellClick);

        tickerView = document.rootVisualElement.Q<ListView>("ticker-list");
        tickerView.makeItem = () => new Label();
        tickerView.bindItem = (element, i) =>
        {
            var myevent = events[i];
            var tmp = $"(D{(int)(myevent.ReceivedTime / (bank.Instance.SecondsOfOpenMarketPerDay + bank.Instance.SecondsOfClosedMarketPerNight))}) " +
                  $"on day {(int)(myevent.info.secondsFromStart / (bank.Instance.SecondsOfOpenMarketPerDay + bank.Instance.SecondsOfClosedMarketPerNight))}, ";
            string tmp2;
            if (myevent.info.quantity > 0)
            {
                tmp2 = $"{myevent.info.quantity} units of {myevent.type} will be sold!";
            }
            else
            {
                tmp2 = $"{-myevent.info.quantity} units of {myevent.type} will be bought!";
            }
            (element as Label).text = tmp + tmp2;
        };
        tickerView.itemsSource = events;

        playerListView = document.rootVisualElement.Q<MultiColumnListView>("player-list");
        playerListView.columns.stretchMode = Columns.StretchMode.GrowAndFill;
        playerListView.columns["name"].makeCell = () => new Label();
        playerListView.columns["networth"].makeCell = () => new Label();
        playerListView.columns["freecash"].makeCell = () => new Label();
        playerListView.columns["trade"].makeCell = () => new Button();
        
        playerListView.columns["name"].bindCell = (element, i) =>
        {
            var offersforme = TradeWithPlayerScreen.Instance.offerIdxsForMe;
            var txt = bank.Instance.playerNames[i].ToString();

            for (int j = 0; j < offersforme.Count; j++)
            {
                if (bank.Instance.allOffers[offersforme[j]].OfferingPlayerId == bank.Instance.playerIds[i])
                {
                    txt += "[offer!]";
                    break;
                }
            }

            (element as Label).text = txt;
        };
        playerListView.columns["networth"].bindCell = (element, i) =>
        {
            (element as Label).text = ComputePlayerNetWorth(i).ToString();
        };
        playerListView.columns["freecash"].bindCell = (element, i) =>
        {
            (element as Label).text = bank.Instance.playerFreeCash[i].ToString();
        };
        playerListView.columns["trade"].bindCell = (element, index) =>
        {
            var button = (element as Button);
            button.text = "TRADE";
            Action cb = () =>
            {
                if (!bank.Instance.gameState.Value.marketOpenNow)
                    ShowTradeWithPlayerView(index);
            };
            button.clicked -= cb;
            button.clicked += cb;
        };
        
        mclv = document.rootVisualElement.Q<MultiColumnListView>("mclv");

        mclv.columns.stretchMode = Columns.StretchMode.GrowAndFill;
        mclv.columns["name"].makeCell = () => new Label();
        mclv.columns["price"].makeCell = () => new Label();
        mclv.columns["position"].makeCell = () => new Label();
        mclv.columns["supply"].makeCell = () => new Label();
        mclv.columns["trade"].makeCell = () => new Button();
                
        mclv.columns["name"].bindCell = (element, index) =>
            (element as Label).text = ((GoodType)index).ToString();
        mclv.columns["price"].bindCell = (element, index) =>
        {
            var currentPrice = bank.Instance.goods[index].price;
            if (bank.Instance.goodsAtMarketOpenToday.Count == 0)
            {
                (element as Label).text = currentPrice.ToString();
                return;
            }

            var dayOpenPrice = bank.Instance.goodsAtMarketOpenToday[index].price;
            var delta = currentPrice - dayOpenPrice;
            string deltastring = delta >= 0 ? $"+{delta}" : delta.ToString();  
            (element as Label).text = $"{currentPrice} ({deltastring})";
        };
        mclv.columns["position"].bindCell = (element, index) =>
        {
            var bgi = bank.Instance.goods[index];

            for (int i = 0; i < bgi.playerPositions.Length; i++)
            {
                if (bgi.playerPositions[i].id == Player.LocalPlayerId())
                {
                    var pos = bgi.playerPositions[i].position;
                    if (bank.Instance.goodsAtMarketOpenToday.Count == 0)
                    {
                        (element as Label).text = pos.ToString();
                        return;
                    }
                    var dayOpenPos = bank.Instance.goodsAtMarketOpenToday[index].playerPositions[i].position;
                    var delta = pos - dayOpenPos;
                    string deltastring = delta >= 0 ? $"+{delta}" : delta.ToString();  
                    (element as Label).text = $"{pos} ({deltastring})";
                    return;
                }
            }
        };
        mclv.columns["supply"].bindCell = (element, index) =>
        {
            var inventory = bank.Instance.goods[index].inventory;
            if (bank.Instance.goodsAtMarketOpenToday.Count == 0)
            {
                (element as Label).text = inventory.ToString();
                return;
            }
            var dayOpenInv = bank.Instance.goodsAtMarketOpenToday[index].inventory;
            var delta = inventory - dayOpenInv;
            string deltastring = delta >= 0 ? $"+{delta}" : delta.ToString();  
            (element as Label).text = $"{inventory} ({deltastring})";
        };
        mclv.columns["trade"].bindCell = (element, index) =>
        {
            (element as Button).text = "Trade";
            tradeButtonShit[element as Button] = index;
            (element as Button).RegisterCallback<ClickEvent>(TradeButtonShitClick);
        };

        twps = new TradeWithPlayerScreen();
        twps.Init();

        gameEndModal = document.rootVisualElement.Q<VisualElement>("GameEndModal");
        gameEndModal.style.display = DisplayStyle.None;
    }

    public void TradeButtonShitClick(ClickEvent evt)
    {
        ShowTradeView((GoodType)tradeButtonShit[evt.target as Button]);
    }

    public void UpdateForNewPlayer()
    {
        mclv.RefreshItems();
        twps.UpdatePlayerList();
    }


    public void ShowTradeWithPlayerView(int playerIdx)
    {
        twps.PlayerIdx = playerIdx;

        twps.PlayerDropDown.index = playerIdx;
        twps.tradeWithPlayerSection.style.display = DisplayStyle.Flex;
        managersSection.style.display = DisplayStyle.None;
    }
    public void HideTradeWithPlayerView()
    {
        twps.tradeWithPlayerSection.style.display = DisplayStyle.None;
        managersSection.style.display = DisplayStyle.Flex;
    }
    

    public float ComputePlayerNetWorth(int playerIdx)
    {
        var ret = 0f;
        for (int i = 0; i < (int)GoodType.NumGoodType; i++)
        {
            var bgi = bank.Instance.goods[i];
            ret += bgi.playerPositions[playerIdx].position * bgi.price;
        }

        ret += bank.Instance.playerFreeCash[playerIdx];
        return ret;
    }

    private void Update()
    {
        if (bank.Instance.IsSpawned &&
            bank.Instance.goods.Count > 0 &&
            bank.Instance.playerIds.Contains(Player.LocalPlayerId()))
        {
            if (myLocalGameStartTime < 0)
                myLocalGameStartTime = Time.time;
            var shitlist = new List<BankGoodInfo>();

            for (int i = 0; i < (int)GoodType.NumGoodType; i++)
            {
                shitlist.Add(bank.Instance.goods[i]);
            }

            mclv.itemsSource = shitlist;
            mclv.RefreshItems();

            var shitlist2 = new List<PlayerGoodInfo>();
            for (int i = 0; i < bank.Instance.goods[0].playerPositions.Length; i++)
            {
                shitlist2.Add(bank.Instance.goods[0].playerPositions[i]);
            }

            playerListView.itemsSource = shitlist2;
            playerListView.RefreshItems();

            tradeSection.Q<Label>("trade-price-label").text =
                bank.Instance.goods[(int)goodToTrade].price.ToString();
            tradeSection.Q<Label>("trade-good-type").text = goodToTrade.ToString();
            tradeSection.Q<Label>("trade-position").text =
                bank.Instance.GetPlayerGoodInfo(goodToTrade, Player.LocalPlayerId()).position.ToString();

            var localPlayerIdx = Player.LocalPlayerIdx();
            localPlayerFreeCashLabel.text = bank.Instance.playerFreeCash[localPlayerIdx].ToString();
            localPlayerNetWorthLabel.text = ComputePlayerNetWorth(localPlayerIdx).ToString();

            var gameState = bank.Instance.gameState.Value;
            var now = Time.time - myLocalGameStartTime;
            

            if (gameState.dayNumber == bank.Instance.NumberOfDaysInGame)
            {
                // game over man
                gameEndModal.style.display = DisplayStyle.Flex;
                gameScreen.style.display = DisplayStyle.None;

                var txt = gameEndModal.Q<Label>("game-end-main-text");
                txt.text = @"Ending net worth by player:\n";

                var networths = new List<float>();
                for (int i = 0; i < bank.Instance.playerIds.Count; i++)
                {
                    networths.Add(ComputePlayerNetWorth(i));
                }

                var sortedIndices = networths.Select((x, i) => new KeyValuePair<float, int>(x, i))
                    .OrderBy(x => -x.Key)
                    .ToList();

                for (int i = 0; i < sortedIndices.Count; i++)
                {
                    txt.text += $"{bank.Instance.playerNames[sortedIndices[i].Value]}: ${sortedIndices[i].Key}\n";
                }

                txt.text += $"Congratulations!";
            }
            else
            {
                var marketChanged = (localMarketOpenNow != gameState.marketOpenNow);
                if (marketChanged && !gameState.marketOpenNow)
                {
                    ShowMainView();
                }

                if (gameState.gameStartTime >= 0)
                {
                    for (int i = events.Count-1; i >=0; i--)
                    {
                        if (events[i].info.secondsFromStart < now)
                        {
                            events.RemoveAt(i);
                        }
                    }
                }
                
                if (marketChanged ||
                    (gameState.gameStartTime >= 0 &&
                     now - dayPercentageLastUpdated > bank.Instance.SecondsBetweenClockUIUpdates))
                {
                    if (marketChanged)
                    {

                        localMarketOpenNow = gameState.marketOpenNow;
                        if (localMarketOpenNow)
                            mclv.style.opacity = 1f;
                        else
                            mclv.style.opacity = .28f;
                    }

                    if (gameState.marketOpenNow)
                        clockLabel.text =
                            $"D{gameState.dayNumber} {(int)gameState.secondsUntilMarketToggles}s until close";
                    else
                    {
                        clockLabel.text =
                            $"D{gameState.dayNumber} {(int)gameState.secondsUntilMarketToggles}s until open";
                    }

                    dayPercentageLastUpdated = now;
                }
            }

            twps.MyUpdate();
        }
    }

    public void ShowTradeView(GoodType goodType)
    {
        goodToTrade = goodType;
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
        startGameButton.style.display = DisplayStyle.Flex;
        tickerSection.style.display = DisplayStyle.None;

    }

    public void OnStartGameClick(ClickEvent evt)
    {
        bank.Instance.GenerateEventsForGameServerRpc();
        
    }

    private void ExitStartScreen()
    {
        startScreen.style.display = DisplayStyle.None;
        gameScreen.style.display = DisplayStyle.Flex;
    }

    public void OnTradeBuyClick(ClickEvent evt)
    {
        Player.LocalPlayer().CmdBuyStockServerRpc(goodToTrade, 1);
    }

    public void OnTradeSellClick(ClickEvent evt)
    {
        Player.LocalPlayer().CmdBuyStockServerRpc(goodToTrade, -1);
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