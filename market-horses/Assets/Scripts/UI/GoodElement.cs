using UnityEngine;
using UnityEngine.UIElements;

public class GoodElement : VisualElement
{
    // Expose the custom control to UXML and UI Builder.
    public new class UxmlFactory : UxmlFactory<GoodElement> { }

    public GoodType goodType;
    // TODO: Do we need to do this every time or can we cache it?
    public Label goodName => this.Q<Label>("good-name");
    public Button tradeButton => this.Q<Button>("trade-button");

    // Custom controls need a default constructor. This default constructor 
    // calls the other constructor in this class.
    public GoodElement() { }

    public GoodElement(GoodType goodType)
    {
        this.goodType = goodType;
        var asset = Resources.Load<VisualTreeAsset>("GoodElement");
        asset.CloneTree(this);

        goodName.text = goodType.ToString();
        tradeButton.RegisterCallback<ClickEvent>(OnTradeClick);
    }

    public void OnTradeClick(ClickEvent evt)
    {
        UIManager.Instance.ShowTradeView(goodType);
    }
}