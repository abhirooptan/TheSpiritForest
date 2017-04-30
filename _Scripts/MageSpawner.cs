using UnityEngine;
using UnityEngine.Networking;

public class MageSpawner : NetworkBehaviour {

    public GameObject magePref;

    public override void OnStartServer()
    {
        //GameObject mage = Instantiate(magePref, transform.position, Quaternion.identity);
        //NetworkServer.Spawn(magePref);
    }
}
