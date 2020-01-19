using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetsDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetWorthText.text = "Net Worth : " + NetWorth;

        
    }

    // Update is called once per frame
    private int NetWorth = 0001;
    private int CashAvailable = 9999;
    private int DayDisplay = 3;
    public Text NetWorthText;
    public Text CashAvailableText;
    public Text DayDisplayText;
    void Update()
    {
        NetWorthText.text = "Net Worth : " + NetWorth;
        CashAvailableText.text = "CASH: " + CashAvailable;
        DayDisplayText.text = $"Day: {DayDisplay}";
    }
}
