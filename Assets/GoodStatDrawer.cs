using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerGoodInfo
{
    GoodType type;
    int position;
}

public class GoodStatDrawer : MonoBehaviour
{
    public GameObject GoodStatRow;
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        var text = GoodStatRow.GetComponent<Text>();
        // var position = GoodStatRow.GetComponent<Text>();
        // var available = GoodStatRow.GetComponent<Text>();
        // var price = GoodStatRow.GetComponent<Text>();
        foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
        {
            var newTextObject = GoodStatRow;
            if (i > 0)
            {
                newTextObject = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            }

            var pos2 = newTextObject.transform.position;
            pos2.y -= 15 * i;
            newTextObject.transform.position = pos2;
            

            var goodPositionText = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            var pos = goodPositionText.transform.position;
            pos.y -= 15 * i;
            pos.x += 50 * 2;
            goodPositionText.transform.position = pos;

            var goodAvailableText = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            pos = goodAvailableText.transform.position;
            pos.y -= 15 * i;
            pos.x += 50 * 3;
            goodAvailableText.transform.position = pos;

            var goodPriceText = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            pos = goodPriceText.transform.position;
            pos.y -= 15 * i;
            pos.x += 50 * 4;
            goodPriceText.transform.position = pos;

            newTextObject.GetComponent<Text>().text = mygoodtype.ToString();
            goodPriceText.GetComponent<Text>().text = $"{mygoodtype} : VAL1     VAL2     VAL3     VAL4";
            goodPositionText.GetComponent<Text>().text = "position";
            goodAvailableText.GetComponent<Text>().text = "available";
            goodPriceText.GetComponent<Text>().text = "price";

            i++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
