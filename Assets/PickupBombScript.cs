using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PickupBombScript : NetworkBehaviour {

	private const string TAG_PLAYER = "player";
	private const string TAG_TERRAIN = "terrain";

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(!isServer)
			return;
		
		var hit = other.gameObject;
		switch (hit.tag)
		{
		case TAG_PLAYER:
			Debug.Log("bombaPickup bateu no player");
			GameObject.FindGameObjectWithTag("powerupspawner").GetComponent<PowerUpSpawner>().PickedUpPowerUp(gameObject.transform.position);
			NetworkServer.Destroy(this.gameObject);
			break;
		case TAG_TERRAIN:
			Debug.Log("bombaPickup bateu no terrain");
			//gameObject.GetComponent<Rigidbody2D>()
			break;

		}
	}
}
