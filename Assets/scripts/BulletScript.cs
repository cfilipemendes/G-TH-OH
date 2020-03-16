using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletScript : NetworkBehaviour {

    
	// Use this for initialization
	void Start () {
        this.GetComponent<TrailRenderer>().sortingLayerName = "bullet";
        this.GetComponent<TrailRenderer>().sortingOrder = 100;
    }

}
