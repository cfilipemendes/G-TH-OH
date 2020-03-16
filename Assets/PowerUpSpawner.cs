using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PowerUpSpawner : NetworkBehaviour {

    public GameObject[] powerups;
    public Transform[] locations;
    ArrayList usedPositions = new ArrayList();

    private float Timer;
    public float INTERVAL;
    
    public int powerupAmount = 0;
    public int MAX_AMOUNT;

    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		if (!isServer)
			return;
		
        if (Timer < Time.time && powerupAmount < MAX_AMOUNT)
        {
            CmdSpawnPowerup();

            powerupAmount++;
            Timer = Time.time + INTERVAL;
        }
    }

    [Command]
    private void CmdSpawnPowerup()
    {
        if (powerupAmount < MAX_AMOUNT)
        {
            Vector3 rnd = locations[UnityEngine.Random.Range(0, locations.Length)].position;
            
            while (usedPositions.Contains(rnd))
            {
                rnd = locations[UnityEngine.Random.Range(0, locations.Length)].position;
            }
            usedPositions.Add(rnd);
            var powerup = (GameObject)Instantiate(powerups[UnityEngine.Random.Range(0, powerups.Length)],
            rnd, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            NetworkServer.Spawn(powerup);
        }
    }

    public void PickedUpPowerUp(Vector3 powerupPos)
	{
		if (!isServer)
		return;
		
        Debug.Log("amount = " + powerupAmount);

        //if(powerupAmount > 0)
        usedPositions.Remove(powerupPos);
        powerupAmount--;

    }

    public override void OnStartServer()
    {
        Timer = Time.time + INTERVAL;
        powerupAmount = 0;
    }


}
