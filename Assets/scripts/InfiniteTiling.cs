using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteTiling : MonoBehaviour {

    public int marginOffset = 2;
    public bool rightTile = false;
    public bool leftTile = false;
    public bool reverseScale = false;

    private float spriteWidth = 0f;
    private Camera cam;
    private Transform thisTransform;

    void Awake()
    {
        cam = Camera.main;
        thisTransform = transform;
    }


	// Use this for initialization
	void Start ()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        spriteWidth = sr.sprite.bounds.size.x;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
        if ( !leftTile || !rightTile )
        {
            float camBounds = cam.orthographicSize * (Screen.width / Screen.height);

            float edgeRight = (thisTransform.position.x + (spriteWidth / 2)) - camBounds;
            float edgeLeft = (thisTransform.position.x - (spriteWidth / 2)) - camBounds;


            float cameraX = cam.transform.position.x;
            if ((cameraX > (edgeRight - marginOffset)) && (!rightTile))
            {
                CreateTile(1);
                rightTile = true;
            }
            else if ((cameraX <= (edgeLeft + marginOffset)) && (!leftTile))
            {
                CreateTile(-1);
                leftTile = true;
            }

        }
	}

    private void CreateTile(int side)
    {

        Vector3 newPos = new Vector3(
            thisTransform.position.x + spriteWidth * side, 
            thisTransform.position.y, 
            thisTransform.position.z
        );
        Transform newTile = (Transform)Instantiate(thisTransform, newPos, thisTransform.rotation);
        if ( reverseScale )
        {
            newTile.localScale = new Vector3(-1 * newTile.localScale.x, newTile.localScale.y, newTile.localScale.z);
        }
        newTile.parent = transform;
        if ( side > 0 )
        {
            newTile.GetComponent<InfiniteTiling>().leftTile = true;
        }
        else
        {
            newTile.GetComponent<InfiniteTiling>().rightTile = true;
        }
    }
}
