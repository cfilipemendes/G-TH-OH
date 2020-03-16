using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Triggers : NetworkBehaviour {
    private const string TAG_BULLET = "bullet";
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLocalPlayer)
            return;

        switch (other.gameObject.tag)
        {
            case TAG_BULLET:
                CmdDestroyBullet(other.gameObject.GetComponent<NetworkIdentity>().netId);   
                if (this.GetComponent<GameState>().currentState == 1)
                {
                    CmdDecreaseHealth();
                    
                }
                
                break;

        }
    }
    [Command]
    void CmdDestroyBullet(NetworkInstanceId id)
    {
        NetworkServer.Destroy(NetworkServer.FindLocalObject(id));
    }
    [Command]
    void CmdDecreaseHealth()
    {
        RpcDecreaseHealth();
    }

    [ClientRpc]
    void RpcDecreaseHealth()
    {
        var health = this.GetComponent<Health>();
        if (health != null)
            health.decreaseHealth(5f);
    }
}
