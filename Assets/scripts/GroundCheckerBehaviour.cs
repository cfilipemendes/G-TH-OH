using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheckerBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D collider)
    {
        if ( collider.tag == "terrain")
        {
            GetComponentInParent<PlayerMovementBehaviour>().GroundCollisionEnter(collider);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if ( collider.tag == "terrain")
        {
            GetComponentInParent<PlayerMovementBehaviour>().GroundCollisionExit(collider);
        }
    }
}
