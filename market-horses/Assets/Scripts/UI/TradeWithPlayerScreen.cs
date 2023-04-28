using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[Serializable]
public class TradeWithPlayerScreen 
{
    public VisualElement tradeWithPlayerSection;
    public VisualElement makeOfferSection;
    public VisualElement receiveOffersSection;
    public Button makeOfferButton;
    public Button receiveOffersButton;

    public MultiColumnListView receivedOffersListView;
    
    public bool twpsIsBuying = true;
    public Button buytoggle;
    public DropdownField GoodDropDown;
    public DropdownField PlayerDropDown;
    public int PlayerIdx;
    public TextField QuantityField;
    public TextField PriceField;
    public Button CancelButton;
    public Button ConfirmButton;
    public GoodType goodToTrade;

    //[SerializeField]
    public Texture2D checkMark;
    //[SerializeField]
    public Texture2D xmark;

    [FormerlySerializedAs("offersForMe")] public List<int> offerIdxsForMe;

    public void Init()
    {
        var uim = UIManager.Instance;
        var document = uim.document;
        offerIdxsForMe = new List<int>();
        
        tradeWithPlayerSection = document.rootVisualElement.Q("trade-with-player-section");
        tradeWithPlayerSection.style.display = DisplayStyle.None;

        makeOfferButton = tradeWithPlayerSection.Q<Button>("make-offer");
        makeOfferButton.RegisterCallback<ClickEvent>(_ =>
        {
            makeOfferSection.style.display = DisplayStyle.Flex;
            receiveOffersSection.style.display = DisplayStyle.None;
        });
        receiveOffersButton = tradeWithPlayerSection.Q<Button>("receive-offers");
        receiveOffersButton.RegisterCallback<ClickEvent>(_ =>
        {
            makeOfferSection.style.display = DisplayStyle.None;
            receiveOffersSection.style.display = DisplayStyle.Flex;
        });
        
        makeOfferSection = tradeWithPlayerSection.Q<VisualElement>("make-offer-section");
        makeOfferSection.style.display = DisplayStyle.Flex;
        receiveOffersSection = tradeWithPlayerSection.Q<VisualElement>("receive-offers-section");
        receiveOffersSection.style.display = DisplayStyle.None;
        
        //===================================
        // make offers section
        //===================================
        buytoggle = tradeWithPlayerSection.Q<Button>("buy-sell-toggle");
        buytoggle.RegisterCallback<ClickEvent>(evt =>
        {
            twpsIsBuying = !twpsIsBuying;
            buytoggle.text = twpsIsBuying ? "Buying" : "Selling";
        });
        
        GoodDropDown = tradeWithPlayerSection.Q<DropdownField>("choose-good");
        var strings = Enum.GetNames(typeof(GoodType)).ToList();
        GoodDropDown.choices = strings.ToList();
        GoodDropDown.RegisterValueChangedCallback(evt =>
        {
            goodToTrade = (GoodType)strings.IndexOf(evt.newValue);
        });

        PlayerDropDown = tradeWithPlayerSection.Q<DropdownField>("choose-player");
        UpdatePlayerList();
        PlayerDropDown.RegisterValueChangedCallback(evt =>
        {
            for (int i = 0; i < bank.Instance.playerNames.Count; i++)
            {
                if (bank.Instance.playerNames[i].ToString().Equals(evt.newValue))
                {
                    PlayerIdx = i;
                    break;
                }
            }

            Debug.Log($"OH NO! COULDNT FIND PLAYER {evt.newValue} FROM DROPDOWN");
        });

        QuantityField = tradeWithPlayerSection.Q<TextField>("quantity-field");
        PriceField = tradeWithPlayerSection.Q<TextField>("price-field");

        CancelButton = tradeWithPlayerSection.Q<Button>("cancel-button");
        CancelButton.RegisterCallback<ClickEvent>(_ => uim.HideTradeWithPlayerView());
        ConfirmButton = tradeWithPlayerSection.Q<Button>("confirm-button");
        ConfirmButton.RegisterCallback<ClickEvent>(evt =>
        {
            var newoffer = new Offer
            {
                count = Int32.Parse(QuantityField.text),
                goodType = goodToTrade,
                OffereePlayerId = bank.Instance.playerIds[PlayerIdx],
                OfferingPlayerId = Player.LocalPlayerId(),
                OfferToBuy = twpsIsBuying,
                price = Single.Parse(PriceField.text)
            };

            bank.Instance.allOffers.Add(newoffer);
            uim.HideTradeWithPlayerView();
        });
        
        //=================================
        // receive offers section
        //================================
        receivedOffersListView = tradeWithPlayerSection.Q<MultiColumnListView>("offers-list");
        receivedOffersListView.columns["player-name"].makeCell = () => new Label();
        receivedOffersListView.columns["good"].makeCell = () => new Label();
        receivedOffersListView.columns["summary"].makeCell = () => new Label();
        receivedOffersListView.columns["cost"].makeCell = () => new Label();
        receivedOffersListView.columns["accept"].makeCell = () => new Button();
        receivedOffersListView.columns["reject"].makeCell = () => new Button();

        receivedOffersListView.columns["player-name"].bindCell = (element, i) =>
        {
            (element as Label).text = bank.Instance.playerNames[Player.PlayerIdxFromId(bank.Instance.allOffers[offerIdxsForMe[i]].OffereePlayerId)]
                .ToString();
        };
        receivedOffersListView.columns["good"].bindCell = (element, i) =>
        {
            (element as Label).text = bank.Instance.allOffers[offerIdxsForMe[i]].goodType.ToString();
        };
        receivedOffersListView.columns["summary"].bindCell = (element, i) =>
        {
            var offer = bank.Instance.allOffers[offerIdxsForMe[i]];
            //with options, this string will get more complicated
            var actionString = offer.OfferToBuy ? "BUY" : "SELL";
            (element as Label).text = $"{actionString} {offer.count} @ {offer.price}";
        };
        receivedOffersListView.columns["cost"].bindCell = (element, i) =>
        {
            var offer = bank.Instance.allOffers[offerIdxsForMe[i]];

            (element as Label).text = (offer.price * offer.count).ToString();
        };
        receivedOffersListView.columns["accept"].bindCell = (element, i) =>
        {
            /*var cb = () =>
                {
                    offersForMe[i].    
                }*/
            var btn = (element as Button);
            btn.text = "Yes";
            Action cb = () =>
            {
                bank.Instance.ConsummateDealServerRpc(offerIdxsForMe[i]);
                UpdateOfferViewShitBasedOnBank();
            };
            btn.clicked -= cb;
            btn.clicked += cb;
        };
        receivedOffersListView.columns["reject"].bindCell = (element, i) =>
        {
            var btn = (element as Button);
            btn.text = "No";
            Action cb = () =>
            {
                Debug.Log($"reject cb {i} {offerIdxsForMe}");
                bank.Instance.allOffers.RemoveAt(offerIdxsForMe[i]);
                UpdateOfferViewShitBasedOnBank();
            };
            btn.clicked -= cb;
            btn.clicked += cb;         
        };
        
    }
    
    
    public void UpdatePlayerList()
    {
        var shitlist = new List<string>();
        foreach (var shit in bank.Instance.playerNames)
            shitlist.Add(shit.Value);
        PlayerDropDown.choices = shitlist;
    }

    public void MyUpdate()
    {
        UpdateOfferViewShitBasedOnBank();
    }

    public void UpdateOfferViewShitBasedOnBank()
    {
        var localplayer = Player.LocalPlayerId();
        offerIdxsForMe.Clear();

        var allOffers = bank.Instance.allOffers;
        for (int i = 0; i < allOffers.Count; i++)
        {
            if (allOffers[i].OffereePlayerId == localplayer)
                offerIdxsForMe.Add(i);
        }

        receivedOffersListView.itemsSource = offerIdxsForMe;
        Debug.Log($"calling refreshitems with length {offerIdxsForMe}");
        receivedOffersListView.RefreshItems();
    }
}