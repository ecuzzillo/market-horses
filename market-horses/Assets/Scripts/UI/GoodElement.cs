using UnityEngine;
using UnityEngine.UIElements;

public class GoodElement : VisualElement
{
    // Expose the custom control to UXML and UI Builder.
    public new class UxmlFactory : UxmlFactory<GoodElement> { }

    public GoodType goodType;
    // TODO: Do we need to do this every time or can we cache it?
    public Label goodName => this.Q<Label>("good-name");
    public Label goodPrice => this.Q<Label>("good-price");
    public Label goodPosition => this.Q<Label>("good-position");
    public Button tradeButton => this.Q<Button>("trade-button");

    // Custom controls need a default constructor. This default constructor 
    // calls the other constructor in this class.
    public GoodElement() { }

    public GoodElement(GoodType goodType)
    {
        this.goodType = goodType;
        var price = -1;//bank.Instance.goods[(int)goodType].price;
        var asset = Resources.Load<VisualTreeAsset>("GoodElement");
        asset.CloneTree(this);

        goodName.text = goodType.ToString();
        goodPrice.text = price.ToString();
        goodPosition.text = "-1";
        tradeButton.RegisterCallback<ClickEvent>(OnTradeClick);
    }

    public void OnTradeClick(ClickEvent evt)
    {
        //UIManager.Instance.ShowTradeView(goodType);
    }
}