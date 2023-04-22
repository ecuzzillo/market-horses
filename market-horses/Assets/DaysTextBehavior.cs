using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DaysTextBehavior : MonoBehaviour
{
    public TMP_Text days;
// Start is called before the first frame update
    void Start()
    {
        days = GetComponent<TextMeshProUGUI>();
        days.text = "Days: ";
    }

    // Update is called once per frame
    void Update()
    {
    days.text = "Days: " + "1";
    }
}
