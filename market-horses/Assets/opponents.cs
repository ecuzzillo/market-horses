using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class opponents : MonoBehaviour
{
    public GameObject Opponent;
    // Start is called before the first frame update
    void Start()
    {
        int i = 0; 
        var person1 = "chris";
        var person2 = "elliot";
        var person3 = "bobby";
        var opponents = new string[] { person1, person2, person3 };

        foreach (var person in (opponents))
        {
            var newButtonObject = Opponent;
            if (i > 0)
            {
                newButtonObject = Instantiate(Opponent, Opponent.transform.parent);

            }
            var pos = newButtonObject.transform.position;
            pos.x += 200 * i;
            newButtonObject.transform.position = pos;
            newButtonObject.GetComponent<Text>().text = person;

            //newButtonObject.SetActive(false);

            i++;

        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
