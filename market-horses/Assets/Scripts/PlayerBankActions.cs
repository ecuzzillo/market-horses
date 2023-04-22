using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerBankActions : MonoBehaviour
{
    // Start is called before the first frame update
    public GoodType type;

    public void BuyStock()
    {
        /*var player = ClientScene.localPlayers.First(p => p.gameObject.GetComponent<NetworkIdentity>().hasAuthority);
        var id = player.gameObject.GetComponent<NetworkIdentity>().netId;
        player.gameObject.GetComponent<Player>().CmdBuyStock(type, id, 1);*/
    }

    public void SellStock(GoodType goodType)
    {
        Debug.Log("Selling");
        Debug.Log("JOE WAS HERE");
        /*var player = ClientScene.localPlayers.First(p => p.gameObject.GetComponent<NetworkIdentity>().hasAuthority);
        var id = player.gameObject.GetComponent<NetworkIdentity>().netId;
        player.gameObject.GetComponent<Player>().CmdBuyStock(type, id, -1);*/
    }

    public void ExerciseFuture()
    {
        Debug.Log("Exercising future");
        }
    public void BuyFuture()
    {
        Debug.Log("Buy future");
     }
}
