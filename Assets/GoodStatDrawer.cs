using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public struct PlayerGoodInfo
{
    public NetworkInstanceId id;
    public int position;
    public FuturePosition[] futurePositions;
}
public struct FuturePosition
{
    float exercisePrice;
    int count;

}

public class GoodStatDrawer : MonoBehaviour
{
    public GameObject GoodStatRow;
    public Dictionary<GoodType, GameObject> GoodNameTexts = new Dictionary<GoodType, GameObject>();
    public Dictionary<GoodType, GameObject> GoodPositionTexts = new Dictionary<GoodType, GameObject>();
    public Dictionary<GoodType, GameObject> GoodInventoryTexts = new Dictionary<GoodType, GameObject>();
    public Dictionary<GoodType, GameObject> GoodPriceTexts = new Dictionary<GoodType, GameObject>();

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
            GoodNameTexts[mygoodtype] = GoodStatRow;
            if (i > 0)
            {
                GoodNameTexts[mygoodtype] = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            }

            var pos2 = GoodNameTexts[mygoodtype].transform.position;
            pos2.y -= 15 * i;
            GoodNameTexts[mygoodtype].transform.position = pos2;
            

            GoodPositionTexts[mygoodtype] = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            var pos = GoodPositionTexts[mygoodtype].transform.position;
            pos.y -= 15 * i;
            pos.x += 50 * 2;
            GoodPositionTexts[mygoodtype].transform.position = pos;

            GoodInventoryTexts[mygoodtype] = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            pos = GoodInventoryTexts[mygoodtype].transform.position;
            pos.y -= 15 * i;
            pos.x += 50 * 3;
            GoodInventoryTexts[mygoodtype].transform.position = pos;

            GoodPriceTexts[mygoodtype] = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
            pos = GoodPriceTexts[mygoodtype].transform.position;
            pos.y -= 15 * i;
            pos.x += 50 * 4;
            GoodPriceTexts[mygoodtype].transform.position = pos;

            GoodNameTexts[mygoodtype].GetComponent<Text>().text = mygoodtype.ToString();
            GoodPriceTexts[mygoodtype].GetComponent<Text>().text = $"{mygoodtype} : VAL1     VAL2     VAL3     VAL4";

            i++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (ClientScene.localPlayers.Count == 0 || bank.Instance.goods.Count == 0)
            return;

        foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
        {
            var info = bank.Instance.goods.First(g => g.type == mygoodtype);

            PlayerGoodInfo playerGoodInfo;
            try
            {
                playerGoodInfo = info.playerPositions.First(
                    p => NetworkServer.FindLocalObject(p.id)?.GetComponent<NetworkIdentity>().hasAuthority ?? false);
            }
            catch (InvalidOperationException e)
            {
                return;
            }
            GoodPositionTexts[mygoodtype].GetComponent<Text>().text = playerGoodInfo.position.ToString();
            GoodInventoryTexts[mygoodtype].GetComponent<Text>().text = info.inventory.ToString();
            GoodPriceTexts[mygoodtype].GetComponent<Text>().text = info.price.ToString();
        }
    }
}
