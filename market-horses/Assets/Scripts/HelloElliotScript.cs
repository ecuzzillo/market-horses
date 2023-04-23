using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HelloElliotScript : MonoBehaviour
{
    public GameObject OpenModal;
    public static string CurrentGoodToTrade;
    public TMP_Text currentName;
    public TMP_InputField tradeCount;
    public TMP_Text CurrentPriceOfGood;
    
    public void ToggleActionModalSilk()
    {
        //gameObject.SetActive(false);
        CurrentGoodToTrade = "Silk";
        currentName.text = "silk";
        //get price of good here
        CurrentPriceOfGood.text = "12";
        
        if (gameObject.active)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    
    }    public void ToggleActionModalCows()
    {
        
        CurrentGoodToTrade = "Cows";
        currentName.text = "Cows";
        //get price of good here
        CurrentPriceOfGood.text = "8";


        //gameObject.SetActive(false);
        if (gameObject.active)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    
    }    public void ToggleActionModalOil()
    {
        CurrentGoodToTrade = "Oil";
        currentName.text = "Oil";
        //get price of good here
        CurrentPriceOfGood.text = "19";

        //gameObject.SetActive(false);
        if (gameObject.active)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    
        Debug.Log("hello darkness, my old friend");
    }  public void ToggleActionModalCottom()
    {
        
        CurrentGoodToTrade = "Cotton";
        currentName.text = "Cotton";
        //get price of good here
        CurrentPriceOfGood.text = "5";
        //gameObject.SetActive(false);
        if (gameObject.active)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    
        Debug.Log("hello darkness, my old friend");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
