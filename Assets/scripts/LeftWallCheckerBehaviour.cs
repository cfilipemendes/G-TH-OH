using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftWallCheckerBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "terrain")
        {
            GetComponentInParent<PlayerMovementBehaviour>().LeftWallCollisionEnter(collider);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == "terrain")
        {
            GetComponentInParent<PlayerMovementBehaviour>().LeftWallCollisionExit(collider);
        }
    }
}
