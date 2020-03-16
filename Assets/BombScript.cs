using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BombScript : NetworkBehaviour {

    private const string TAG_PLAYER = "player";
    private const string TAG_TERRAIN = "terrain";
    private GameObject hitMan;

    void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        gameObject.GetComponent<Animator>().enabled = false;
        StartCoroutine(ActivateTrap("whatevs", 1));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var hit = other.gameObject;
        switch (hit.tag)
        {
            case TAG_PLAYER:
                Debug.Log("bomba bateu no player");
                var health = hit.GetComponent<Health>();

                if (health != null)
                {
                    health.decreaseHealth(50f);
                }
                if (hit.GetComponent<Rigidbody2D>() != null)
                {
                    Debug.Log("Get pushed m8");
                    Debug.Log("|" + (this.transform.position.x - hit.transform.position.x) + "|");
                    Debug.Log("|" + (this.transform.position.y - hit.transform.position.y) + "|");
                    hitMan = hit;
                    StartCoroutine(knockBack("whatevs", 0.001f));
                    //hit.GetComponent<Rigidbody2D>().AddForce(-hit.GetComponent<Rigidbody2D>().velocity, ForceMode2D.Impulse);
                    //hit.GetComponent<Rigidbody2D>().AddForce(new Vector2((hit.transform.position.x - this.transform.position.x)* 25f,
                                                            //(hit.transform.position.y - this.transform.position.y)* 25f), ForceMode2D.Impulse);
                }

                gameObject.GetComponent<SpriteRenderer>().enabled = true;
                gameObject.GetComponent<Animator>().enabled = true;
                // tira vida se tiver a coroa
                
                StartCoroutine(DestroyBomb("whatevs", 0.5f));


                // play sound
                hit.GetComponent<PlayerMovementBehaviour>().CmdPlayExplosionSound();
                break;
            case TAG_TERRAIN:
                Debug.Log("bomba bateu no terrain");
                break;
        }
    }

    IEnumerator ActivateTrap(string powerUpUsed, float delay)
    {
        yield return new WaitForSeconds(delay);
        // activates the trap collider after 1sec
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        
    }

    IEnumerator DestroyBomb(string powerUpUsed, float delay)
    {
        yield return new WaitForSeconds(delay);
        // activates the trap collider after 1sec
        NetworkServer.Destroy(gameObject);

    }

    IEnumerator knockBack(string powerUpUsed, float delay)
    {
        yield return new WaitForSeconds(delay);
        hitMan.GetComponent<Rigidbody2D>().AddForce(-hitMan.GetComponent<Rigidbody2D>().velocity, ForceMode2D.Impulse);
        hitMan.GetComponent<Rigidbody2D>().AddForce(new Vector2((hitMan.transform.position.x - this.transform.position.x)* 25f,
        (hitMan.transform.position.y - this.transform.position.y)* 25f), ForceMode2D.Impulse);
    }
}
