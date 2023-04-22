using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class NetWorthBehavior: MonoBehaviour
{
    public TMP_Text networthTest;

    void Start()
    {
        networthTest = GetComponent<TextMeshProUGUI>();
        networthTest.text = "100";
    }

    void Update()
    {
        //networthTest.text = "100";
    }
}
