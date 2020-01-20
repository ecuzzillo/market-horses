using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public struct PlayerGoodInfo
{
    public NetworkIdentity id;
    public int position;
    public List<FuturePosition> futurePositions;
}
public struct FuturePosition
{
    float exercisePrice;
    int count;

}

public class GoodStatDrawer : MonoBehaviour
{
    public GameObject GoodStatRow;
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        var text = GoodStatRow.GetComponent<Text>();
        foreach (var mygoodtype in (GoodType[])Enum.GetValues(typeof(GoodType)))
        {
            if (i > 0)
            {
                var newTextObject = Instantiate(GoodStatRow, GoodStatRow.transform.parent);
                var pos = newTextObject.transform.position;
                pos.y -= 15 * i;
                newTextObject.transform.position = pos;
                text = newTextObject.GetComponent<Text>();
            }
            text.text = $"DONT FEED ME {mygoodtype} SEYMOUR";
            i++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (ClientScene.localPlayers.Count > 0)
        Debug.Log($"ClientScene has {ClientScene.localPlayers[0].gameObject.name}");
    }
}
