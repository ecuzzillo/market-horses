using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument document;
    public VisualElement tickerSection => document.rootVisualElement.Q("ticker-section");
    public VisualElement managersSection => document.rootVisualElement.Q("managers-section");
    public VisualElement goodsSection => document.rootVisualElement.Q("goods-section");
    public VisualElement tradeSection => document.rootVisualElement.Q("trade-section");

    public GoodType goodToTrade;

    public static UIManager Instance;

    public void Start()
    {
        Instance = this;

        document = GetComponent<UIDocument>();

        var asset = Resources.Load<VisualTreeAsset>("Main");
        asset.CloneTree(document.rootVisualElement);

        foreach (var goodType in (GoodType[]) Enum.GetValues(typeof(GoodType)))
        {
            var goodElement = new GoodElement(goodType);
            document.rootVisualElement.Q("goods-list").Add(goodElement);
        }

        tradeSection.style.display = DisplayStyle.None;

        document.rootVisualElement.Q("buy-button").RegisterCallback<ClickEvent>(OnTradeBuyClick);
        document.rootVisualElement.Q("sell-button").RegisterCallback<ClickEvent>(OnTradeSellClick);
    }

    public void ShowTradeView(GoodType goodType)
    {
        goodToTrade = goodType;
        managersSection.style.display = DisplayStyle.None;
        goodsSection.style.display = DisplayStyle.None;
        tradeSection.style.display = DisplayStyle.Flex;
    }

    public void OnTradeBuyClick(ClickEvent evt)
    {
        ShowMainView();
    }

    public void OnTradeSellClick(ClickEvent evt)
    {
        ShowMainView();
    }

    public void ShowMainView()
    {
        managersSection.style.display = DisplayStyle.Flex;
        goodsSection.style.display = DisplayStyle.Flex;
        tradeSection.style.display = DisplayStyle.None;
    }
}