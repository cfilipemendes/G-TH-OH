using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StartingCrownSpawn : NetworkBehaviour {


	public GameObject crownObject;
    public Transform[] locations;

    public override void OnStartServer(){
        createCrown();
    }

    public void createCrown()
    {
        var crown = (GameObject)Instantiate(crownObject, locations[UnityEngine.Random.Range(0, locations.Length)].position, Quaternion.Euler(
            0.0f,
            0.0f,
            0.0f));

        NetworkServer.Spawn(crown);
    }
}
