using UnityEngine;
using System.Collections;

public class Good : MonoBehaviour
{
	public GoodType goodType;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
			
	}

    public void BuyStock()
	{
		Debug.Log("Buying");
		FindAnyObjectByType<bank>().BuyStockServerRpc(goodType, Player.LocalPlayerId(), 1);
    }
}

