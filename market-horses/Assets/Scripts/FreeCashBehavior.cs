using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FreeCashBehavior : MonoBehaviour
{
    public TMP_Text freeCash;

    void Start()
    {
        freeCash = GetComponent<TextMeshProUGUI>();
        freeCash.text = "100";
    }

    void Update()
    {
    }
}
