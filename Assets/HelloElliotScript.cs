using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloElliotScript : MonoBehaviour
{
    public GameObject OpenModal;


    public void MyMessage()
    {
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
