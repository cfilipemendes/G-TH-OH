using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameState : NetworkBehaviour {

	// verify if i have the crown
	public const int holyGrailOwnerState = 0;

	[SyncVar]
	public NetworkInstanceId crownOwner;
    
	public int currentState = holyGrailOwnerState;

	public bool gameEnded = false;
    public String powerup;

	void Update(){

	}

	public void onCrownCatch(){

		if (!isLocalPlayer)
			return;

        CmdConfirmationOwner(this.gameObject.GetComponent<NetworkIdentity>().netId);

        currentState = 1;
		Debug.Log ("I've got the crown now!");
	}

    [ClientRpc]
	public void RpcOnCrownLost(){

		if (!isLocalPlayer)
			return;

        currentState = 0;
		Debug.Log ("Shiet i just lost the crown!!");
	}

	[Command]
	void CmdConfirmationOwner(NetworkInstanceId id){
        crownOwner = id;
        Debug.Log("O owner da coroa eh o:" + crownOwner);
	}

    public int getCurrentState()
    {
        return currentState;
    }

    public void Powerup(string powerup)
    {
        Debug.Log("I got meself a bemb!");
        this.powerup = powerup;
    }

	[ClientRpc]
	public void RpcSetGameEnd(bool state){
		gameEnded = state;
	}

}