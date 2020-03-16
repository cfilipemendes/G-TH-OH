using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletBehaviour : NetworkBehaviour {

    private const string TAG_PLAYER = "player";
    private const string TAG_TERRAIN = "terrain";
    private const string TAG_SHIELD = "shield";

    public const float BULLET_KNOCKBACK = 50f;
    public const float BULLET_DMG = 20f;
	public NetworkInstanceId shooterId;
	private float spawnTime;
    // Use this for initialization
    void Start () {
        GetComponent<TrailRenderer>().sortingLayerName = "effects";
        GetComponent<TrailRenderer>().sortingOrder = 101;
		spawnTime = Time.time;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D other)
    {



        var hit = other.gameObject;
        switch (hit.tag)
        {
		case TAG_PLAYER:               

			if ((Time.time  - spawnTime) < 0.03f)
				return;

			
                var health = hit.GetComponent<Health>();
                if (health != null)
                {
                    health.decreaseHealth(BULLET_DMG);                    
                }
                if ( hit.GetComponent<Rigidbody2D>() )
               	    hit.GetComponent<Rigidbody2D>().AddForce(gameObject.transform.right * BULLET_KNOCKBACK, ForceMode2D.Impulse);

                NetworkServer.Destroy(gameObject);

                // apply juicing (camera shake)
                //.GetComponent<CameraShake>().ShakeCamera(5.0f, 0.2f);

                break;
            case TAG_TERRAIN:
              //NetworkServer.Destroy(gameObject);
            break;
            case TAG_SHIELD:
                NetworkServer.Destroy(gameObject);
            break;
        }
    }

    
}
