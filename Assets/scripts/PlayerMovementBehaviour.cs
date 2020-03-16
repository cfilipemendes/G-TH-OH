using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerMovementBehaviour : NetworkBehaviour {


    //
    // Parameters
    // ------------------------------------------------------------------------
    [Header("Player Movement Parameters")]
    public float acceleration     = 400f;
    public float jumpAcceleration = 15f;
    public float maxVelocity      = 100f;
    public float dashAcceleration = 150f;
    public float dashTime         = 0.8f;

    [Header("Player Firing Parameters")]
    public float bulletSpeed = 100f;
    public GameObject arm;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    [Header("Player Powerups Parameters")]
    public GameObject basicShield;
    public GameObject bombPrefab;
    public Transform trapSpawn;

    [Header("Stamina Costs")]
    public float DASH_COST = 150f;
    public float SHOOT_COST = 15f;
    public float SHIELD_COST = 200f;

    [Header("Player sounds")]
    public AudioClip audioJump;
    public AudioClip audioShoot;
    public AudioClip audioDash;
    public AudioClip audioHurt;
    public AudioClip audioExplosion;
    public AudioClip audioPowerup;

    [Header("UI Events such as powerups")]
    //Text used to display used powerups and such
    public Text eventFeedback; 

    //
    // Fields
    // ------------------------------------------------------------------------
    private Camera mainCamera;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private ParticleSystem dashParticles;
    private TrailRenderer trailRenderer;
    private Animator animator;
    private float lookAtAngle = 0.0f;
    private bool grounded = false;
    private bool dashing = false;
    private bool leftWallJumping = false;
    private bool rightWallJumping = false;
    private int jumpsMade = 0;
    private float shieldWaitSeconds = SHIELD_WAIT_TIME; // in seconds

    private const float CAMERA_MIN_SIZE = 20f;
    private const float CAMERA_MAX_SIZE = 35f;

    private const float SHIELD_WAIT_TIME = 2f; // in seconds

    private const string STAMINA_POWERUP_TAG = "stamina";
    private const string BOMB_USED_TAG = "bomb";
    private const string BOMB_PICKUP_TAG = "pickedBomb";

    // for activation purposes on the co routine
    private GameObject trap;

	//FIELD TO RESPAWN THE PLAYER
	private NetworkStartPosition[] spawnPoints;

    private bool waitingForRespawn = false;

    //
    // Events
    // ------------------------------------------------------------------------

    /// <summary>
    /// When the entity is started
    /// </summary>
    void Start () {
        eventFeedback.enabled = false;
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.sortingLayerName = "background_effects";
        trailRenderer.sortingOrder = 100;

        mainCamera = Camera.main;
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dashParticles = GetComponentInChildren<ParticleSystem>();
        animator = GetComponent<Animator>();
        dashParticles.Stop();

        if (!isLocalPlayer)
            return;

		spawnPoints = FindObjectsOfType<NetworkStartPosition>();
    }
	
	/// <summary>
    /// 
    /// </summary>
	void Update () {

        if (!isLocalPlayer)
            return;

        // calculate the mouse position & angle
        float camDis = mainCamera.transform.position.y - transform.position.y;
        Vector3 mouse = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, camDis));
        float AngleRad = Mathf.Atan2(mouse.y - transform.position.y, mouse.x - transform.position.x);
        lookAtAngle = (180 / Mathf.PI) * AngleRad;

        // invert image
        if (Mathf.Abs(lookAtAngle) < 90)
        {
            spriteRenderer.flipX = false;
			CmdChangeMyFlip (false);
        }
        else
        {
            spriteRenderer.flipX = true;
			CmdChangeMyFlip(true);
        }

        // set vector of transform directly
        foreach ( Transform t in transform )
        {
            if ( t.tag == "player_arm")
                t.rotation = Quaternion.Euler(0, 0, lookAtAngle);
        }


        // check input events
        CheckInputEvents();

        // shoot
        if (Input.GetMouseButtonDown(0))
        {
            if (gameObject.GetComponent<Stamina>().decreaseStamina(SHOOT_COST))
                CmdFire();
        }

        // shield
        if (Input.GetMouseButton(1))
        {
            if (shieldWaitSeconds >= SHIELD_WAIT_TIME)
            {
                if (gameObject.GetComponent<Stamina>().decreaseStaminaOverTime(SHIELD_COST))
                    CmdChangeMyShield(true);
                else
                {
                    CmdChangeMyShield(false);
                    shieldWaitSeconds = 0.0f;
                    gameObject.GetComponent<Stamina>().stopDecreasingStamina();
                }
            }
            else
            {
                CmdChangeMyShield(false);
                gameObject.GetComponent<Stamina>().stopDecreasingStamina();
            }

        }
        if ( Input.GetMouseButtonUp(1))
        {
            CmdChangeMyShield(false);
            gameObject.GetComponent<Stamina>().stopDecreasingStamina();
        }
 
        // check shield time
        if ( shieldWaitSeconds < SHIELD_WAIT_TIME )
        {
            shieldWaitSeconds += Time.deltaTime;
        }


    }
    
    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {

        if (!isLocalPlayer)
            return;


        // update camera (smoother than on LateUpdate for some reason)
        if (!mainCamera.GetComponent<CameraBoundBehaviour>().outOfBounds)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z), 3f * Time.deltaTime);
            float newOrthographicSize = Mathf.Clamp(mainCamera.orthographicSize * (rigidBody.velocity.magnitude * 0.05f), CAMERA_MIN_SIZE, CAMERA_MAX_SIZE);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, newOrthographicSize, 3f * Time.deltaTime);
        }
        else
        {
            if (!waitingForRespawn)
            {
                if (gameObject.GetComponent<GameState>().currentState == 1)
                {
                    // warn every player
                    this.gameObject.GetComponent<PlayerMovementBehaviour>().CmdShowText("The Holy Grail has been dropped.", 3f);
                    CmdKillPlayer();
                }
                

                StartCoroutine(ShowSpecificMessage("You have died. You spawn in 5 seconds.", 5f));
                waitingForRespawn = true;
                gameObject.GetComponent<Stamina>().resetStamina();

                // teleport him far away!
                rigidBody.position = new Vector3(0, -1000f, 0);
                StartCoroutine(RespawnPlayer(5f));
            }
        }

   
        // check if is moving vertically
        if (rigidBody.velocity.y > 0.01f)
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", true);
        }
        else if (rigidBody.velocity.y < -0.01f)
        {
            animator.SetBool("isFalling", true);
            animator.SetBool("isJumping", false);
        }
        else
        {
            animator.SetBool("isGrounded", true);
            animator.SetBool("isFalling", false);
            animator.SetBool("isJumping", false);
        }

        // check if moving horizontally
        if (Mathf.Abs(rigidBody.velocity.x) > 0.01f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

    }

    
    /// <summary>
    /// 
    /// </summary>
    void LateUpdate()
    {

        if (!isLocalPlayer)
            return;
    }

    public void GroundCollisionEnter(Collider2D collider)
    {
        grounded = true;
        jumpsMade = 0;
        animator.SetBool("isGrounded", true);
    }

    public void GroundCollisionExit(Collider2D collider)
    {
        grounded = false;
        jumpsMade = 1;
        animator.SetBool("isGrounded", false);
    }

    public void LeftWallCollisionEnter(Collider2D collider)
    {
        leftWallJumping = true;
    }

    public void LeftWallCollisionExit(Collider2D collider)
    {
        leftWallJumping = false;
    }

    public void RightWallCollisionEnter(Collider2D collider)
    {
        rightWallJumping = true;
    }

    public void RightWallCollisionExit(Collider2D collider)
    {
        rightWallJumping = false;
    }

    //
    // Functions
    // ------------------------------------------------------------------------

    private void CheckInputEvents()
    {
        // press A
        if (Input.GetKey(KeyCode.A))
        {
            if(-rigidBody.velocity.x < maxVelocity)
            {
                if (grounded)
                {
                    rigidBody.AddForce(new Vector2(-acceleration * Time.deltaTime, 0), ForceMode2D.Force);
                }
                else
                {
                    rigidBody.AddForce(new Vector2(-acceleration * 0.5f * Time.deltaTime, 0), ForceMode2D.Force);
                }
            }
        }
        // press D
        if (Input.GetKey(KeyCode.D))
        {
            if (rigidBody.velocity.x < maxVelocity)
            {
                if (grounded)
                {
                    rigidBody.AddForce(new Vector2(acceleration * Time.deltaTime, 0), ForceMode2D.Force);
                }
                else
                {
                    rigidBody.AddForce(new Vector2(acceleration * 0.5f * Time.deltaTime, 0), ForceMode2D.Force);
                }
            }
        }

        // press S
        if (Input.GetKey(KeyCode.S))
        {
            if ( !grounded )
            {
                if ( -rigidBody.velocity.y < maxVelocity )
                    rigidBody.AddForce(new Vector2(0, -acceleration * Time.deltaTime), ForceMode2D.Force);
            }

        }

        // press jump
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            if (grounded)
            {
                animator.SetBool("isJumping", true);
                rigidBody.AddForce(new Vector2(0, jumpAcceleration), ForceMode2D.Impulse);
                jumpsMade++;

                // apply juicing
                CmdPlayJumpSound();
            }
            else
            {
                // is jumping from a left wall?
                if (leftWallJumping)
                {
                    rigidBody.AddForce(-rigidBody.velocity, ForceMode2D.Impulse);

                    Vector2 leftForce = (Vector2.right * jumpAcceleration) + new Vector2(0, jumpAcceleration);
                    rigidBody.AddForce(leftForce, ForceMode2D.Impulse);

                    // apply juicing
                    CmdPlayJumpSound();
                }
                // is jumping from a right wall?
                else if ( rightWallJumping )
                {
                    rigidBody.AddForce(-rigidBody.velocity, ForceMode2D.Impulse);

                    Vector2 rightForce = (Vector2.left * jumpAcceleration) + new Vector2(0, jumpAcceleration);
                    rigidBody.AddForce(rightForce, ForceMode2D.Impulse);

                    // apply juicing
                    CmdPlayJumpSound();
                }
                // jumping normally
                else
                {
                    if (jumpsMade < 2)
                    {
                        animator.SetBool("isJumping", true);
                        rigidBody.AddForce(new Vector2(0, -rigidBody.velocity.y), ForceMode2D.Impulse);
                        rigidBody.AddForce(new Vector2(0, jumpAcceleration), ForceMode2D.Impulse);
                        jumpsMade++;

                        // apply juicing
                        CmdPlayJumpSound();
                    }
                }
            }
        }

        // press dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!dashing)
            {
                if (gameObject.GetComponent<Stamina>().decreaseStamina(DASH_COST)) { 
                    rigidBody.AddForce(-rigidBody.velocity, ForceMode2D.Impulse);
                    rigidBody.AddForce(arm.transform.right * dashAcceleration, ForceMode2D.Impulse);
                    //dashParticles.Play();
                    //CmdChangeMyDashParticle(true);
                    mainCamera.GetComponent<CameraShake>().ShakeCamera(5f, dashTime);
                    StartCoroutine(StopDash());

                    // apply juicing
                    CmdPlayDashSound();
                    ToggleSpecialEffects(true);
                }
            }
        }

        // press powerup
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (grounded)
            {
                // TODO: SWITCH NO TIPO DE POWER UP USADO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                bool usedPowerup = false;

                if (this.gameObject.GetComponent<GameState>().powerup.Equals("bomb"))
                {
                    usedPowerup = true;
                    CmdPutTrap();
                    StartCoroutine(ShowMessage(BOMB_USED_TAG, 3));
                }

                // remove powerup on use
                if (usedPowerup)
                    this.gameObject.GetComponent<GameState>().powerup = ""; 
            }
        }
}

    //used when player goes out of bounds
    [Command]
    void CmdKillPlayer()
    {
        gameObject.GetComponent<Health>().decreaseHealth(100f);
    }


    [Command]
    void CmdChangeMyFlip(bool change)
    {
        RpcFlipChange(change);
    }

    [ClientRpc]
    void RpcFlipChange(bool change)
    {
        GetComponent<SpriteRenderer>().flipX = change;
    }


    [Command]
    void CmdChangeMyDashParticle(bool change)
    {
        RpcFlipChangeMyDashParticle(change);
    }

    [ClientRpc]
    void RpcFlipChangeMyDashParticle(bool change)
    {
        if (change)
        {
            GetComponentInChildren<ParticleSystem>().Play();
        }
        else
        {
            GetComponentInChildren<ParticleSystem>().Stop();
        }

    }

    [Command]
    void CmdChangeMyShield(bool change)
    {
        RpcChangeShield(change);
    }

    [ClientRpc]
    void RpcChangeShield(bool change)
    {
        
        basicShield.GetComponent<PolygonCollider2D>().enabled = change;
        basicShield.GetComponent<SpriteRenderer>().enabled = change;
        
    }

    [Command]
    public void CmdPlayJumpSound()
    {
        RpcPlayJumpSound();
    }

    [ClientRpc]
    void RpcPlayJumpSound()
    {
        AudioSource.PlayClipAtPoint(audioJump, transform.position, 1.0f);
    }

    [Command]
    public void CmdPlayShootSound()
    {
        RpcPlayShootSound();
    }

    [ClientRpc]
    void RpcPlayShootSound()
    {
        AudioSource.PlayClipAtPoint(audioShoot, transform.position, 1.0f);
    }

    [Command]
    public void CmdPlayDashSound()
    {
        RpcPlayDashSound();
    }

    [ClientRpc]
    void RpcPlayDashSound()
    {
        AudioSource.PlayClipAtPoint(audioDash, transform.position, 1.0f);
    }

    [Command]
    public void CmdPlayExplosionSound()
    {
        RpcPlayExplosionSound();
    }

    [ClientRpc]
    void RpcPlayExplosionSound()
    {
        AudioSource.PlayClipAtPoint(audioExplosion, transform.position, 1.0f);
    }

    [Command]
    public void CmdPlayPowerupSound()
    {
        RpcPlayPowerupSound();
    }

    [ClientRpc]
    void RpcPlayPowerupSound()
    {
        AudioSource.PlayClipAtPoint(audioPowerup, transform.position, 1.0f);
    }

    // fire some bulleetzzz
    [Command]
    void CmdFire()
    {
		var bullet = (GameObject)Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        //Transform arm = GameObject.Find("player_arm").transform;
        //bullet.GetComponent<Rigidbody2D>().AddForce(arm.right * 50, ForceMode2D.Impulse);

		bullet.GetComponent<Rigidbody2D>().AddForce(arm.transform.right * bulletSpeed, ForceMode2D.Impulse);
		bullet.GetComponent<BulletBehaviour> ().shooterId = gameObject.GetComponent<NetworkIdentity> ().netId;
        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 5.0f);

        // game juicing, apply sound
        CmdPlayShootSound();
    }

    //Lay down some trapzzz
    [Command]
    void CmdPutTrap()
    {
        trap = (GameObject)Instantiate(bombPrefab, trapSpawn.position, trapSpawn.rotation);

        // Spawn the trap on the Clients
        NetworkServer.Spawn(trap);
        
    }

    IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashTime);
        rigidBody.AddForce(-rigidBody.velocity, ForceMode2D.Impulse);
        //CmdChangeMyDashParticle(false);
        //dashParticles.Stop();
        dashing = false;
        ToggleSpecialEffects(false);
    }


    public IEnumerator ShowMessage(string powerUpUsed, float delay)
    {
        // switch nos varios tipos de powerups usados
        eventFeedback.enabled = true;
        switch (powerUpUsed)
        {
            case BOMB_USED_TAG:
                eventFeedback.text = "Bomb Deployed! Now dont forget about it!;)";
                break;

            case STAMINA_POWERUP_TAG:
                eventFeedback.text = "Stamina Replenished!";
            break;

            case BOMB_PICKUP_TAG:
                eventFeedback.text = "You picked up a bomb! Use it wisely!";
            break;
        }

        yield return new WaitForSeconds(delay);
        eventFeedback.enabled = false;
    }

    public IEnumerator ShowSpecificMessage(string msg, float delay)
    {
        eventFeedback.enabled = true;
        eventFeedback.text = msg;
        yield return new WaitForSeconds(delay);
        eventFeedback.enabled = false;
    }

    [Command]
    public void CmdShowText(string text, float delay)
    {
        RpcShowText(text, delay);
    }

    [ClientRpc]
    void RpcShowText(string text, float delay)
    {
        StartCoroutine(ShowSpecificMessage(text, delay));
    }

    private void ToggleSpecialEffects(bool change)
    {
        mainCamera.GetComponent<UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration>().enabled = change;
        mainCamera.GetComponent<UnityStandardAssets.ImageEffects.MotionBlur>().enabled = change;
    }

    private IEnumerator RespawnPlayer(float time)
    {
        mainCamera.GetComponent<UnityStandardAssets.ImageEffects.Grayscale>().enabled = true;
        yield return new WaitForSeconds(time);
        transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        mainCamera.GetComponent<CameraBoundBehaviour>().outOfBounds = false;
        waitingForRespawn = false;
        mainCamera.GetComponent<UnityStandardAssets.ImageEffects.Grayscale>().enabled = false;

    }
}
