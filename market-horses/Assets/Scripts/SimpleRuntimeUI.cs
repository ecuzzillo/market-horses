using UnityEngine;
using UnityEngine.UIElements;

public class SimpleRuntimeUI : MonoBehaviour
{
    private Button _button;
    private Toggle _toggle;

    private int _clickCount;

    //Add logic that interacts with the UI controls in the `OnEnable` methods
    private void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        _button = uiDocument.rootVisualElement.Q(className: "cows-trade-button") as Button;
        _button.RegisterCallback<ClickEvent>(CowsTradeButtonHandler);
        
        _button = uiDocument.rootVisualElement.Q(className: "cotton-trade-button") as Button;
        _button.RegisterCallback<ClickEvent>(CottonTradeButtonHandler);
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(CowsTradeButtonHandler);
        _button.UnregisterCallback<ClickEvent>(CottonTradeButtonHandler);
    }

    public void CottonTradeButtonHandler(ClickEvent evt)
    {
        Debug.Log("wtf");
        var bank = FindAnyObjectByType<bank>();
        bank.BuyStockServerRpc(GoodType.Cotton, Player.LocalPlayerId(), 5);

    }

    public void CowsTradeButtonHandler(ClickEvent evt)
    {
        var uiDocument = GetComponent<UIDocument>();
        var fundManagers = uiDocument.rootVisualElement.Q(className: "fund-managers");
        var displayStyle = fundManagers.style.display;
        if (displayStyle == null || displayStyle == DisplayStyle.Flex)
        {
            fundManagers.style.display = DisplayStyle.None;
        }
        else
        {
            fundManagers.style.display = DisplayStyle.Flex;
        }
    }
}