using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundBehaviour : MonoBehaviour {

    public Transform upperBound;
    public Transform lowerBound;
    public bool outOfBounds = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        Vector3 cameraPos = Camera.main.transform.position;

        // reached any of the bounds?
        if ( cameraPos.x >= upperBound.position.x || cameraPos.y >= upperBound.position.y 
            || cameraPos.x <= lowerBound.position.x || cameraPos.y <= lowerBound.position.y )
        {
            outOfBounds = true;
        }
        else
        {
            outOfBounds = false;
        }
	}
}
