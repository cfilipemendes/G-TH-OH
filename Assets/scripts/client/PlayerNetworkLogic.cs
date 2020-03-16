using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerNetworkLogic : NetworkBehaviour {

	private const string TAG_PLAYER  = "player";
	private const string TAG_TERRAIN = "terrain";
	private const string TAG_PROPS = "props";
	private const string TAG_HOLYGRAIL = "holy_grail";
    private const string TAG_POWERUP = "powerup";
    private const string STAMINA_POWERUP_TAG = "stamina";
    private const string BOMB_PICKUP_TAG = "pickedBomb";

    [SyncVar]
	private NetworkInstanceId holyGrailOwner;

    public RectTransform healthBar;
    public Text playerNickname;
    public SpriteRenderer playerCrownSprite;
    public TrailRenderer playerCrownTrail;
    public ParticleSystem playerCrownParticles;


    private string playerName;

	/// <summary>
	/// When the entity starts
	/// </summary>
	void Start()
	{

        playerName = PlayerPrefs.GetString("nickname");
        CmdSetMyNickname(playerName);
        playerCrownTrail.sortingLayerName = "effective";
        playerCrownTrail.sortingOrder = 100;

		// is it a local player?
		if (!isLocalPlayer)
			return;
        healthBar.gameObject.SetActive(false);
    }

	/// <summary>
	/// When the entity is updated
	/// </summary>
	void Update()
	{
		if (!isLocalPlayer)
			return;

		//1ST OF ALL --> CHECK IF I WON

		if (this.gameObject.GetComponent<GameState> ().gameEnded) {
			CmdEndGame ();
		}

        //then lets go with the logic

        // am I the holy grail owner?
        if (this.gameObject.GetComponent<GameState> ().currentState == 1) {
			Debug.Log ("Im a motherfucking starboy!");
            //CmdChangeMyColor();
            CmdChangePlayerCrownSprite(true);
			CmdDecreaseImpurity();
            CmdShowHealthBar();
        }

		// I am not? god damn it
		else
		{
            CmdResetHealthBar();
            //CmdResetMyColor();
            CmdChangePlayerCrownSprite(false);
            //Debug.Log("I am not the owner.");
        }

        // update name
        CmdSetMyNickname(playerName);
    }

	/// <summary>
	/// Ennding the game
	/// </summary>
	[Command]
	public void CmdEndGame (){
		RpcEndGame ();
	}

	[ClientRpc]
	public void RpcEndGame (){
		if (gameObject.GetComponent<GameState> ().currentState == 1) {
			SceneManager.LoadScene ("Win_Scene");
		} else {
			SceneManager.LoadScene ("Lose_Scene");
		}
		Network.Disconnect ();
	}
	/// <summary>
	/// End on game Ending
	/// </summary>

    [Command]
    void CmdShowHealthBar()
    {
        RpcShowHealthBar();
    }

    [ClientRpc]
    void RpcShowHealthBar()
    {
        healthBar.gameObject.SetActive(true);
    }

    [Command]
    void CmdResetHealthBar()
    {
        RpcResetHealthBar();
    }

    [ClientRpc]
    void RpcResetHealthBar()
    {
        healthBar.gameObject.SetActive(false);
        this.GetComponent<Health>().resetHealth();
    }

    [Command]
    void CmdDecreaseImpurity()
    {
        RpcDecreaseImpurity();
    }

    [ClientRpc]
    void RpcDecreaseImpurity()
    {
        var impurity = this.GetComponent<Impurity>();
        if (impurity != null)
            impurity.decreaseImpurity(0.025f);
    }

    [Command]
	void CmdChangeMyColor(){
		RpcSetColor (new Color(1.0f, 0.0f, 0.0f, 0.5f));
	}

    [Command]
    public void CmdResetMyColor()
    {
        RpcSetColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
    }

    [ClientRpc]
	void RpcSetColor(Color col){		
		this.GetComponent<SpriteRenderer> ().color = col;
	}

    [Command]
    public void CmdSetMyNickname(string nick)
    {
        RpcSetMyNickname(nick);
    }

    [ClientRpc]
    void RpcSetMyNickname(string nick)
    {
        playerNickname.text = nick;
    }

    [Command]
    void CmdChangePlayerCrownSprite(bool change)
    {
        RpcChangePlayerCrownSprite(change);
    }

    [ClientRpc]
    void RpcChangePlayerCrownSprite(bool change)
    {
        playerCrownSprite.enabled = change;
        playerCrownTrail.enabled = change;

        if (change)
            playerCrownParticles.Play();
        else
            playerCrownParticles.Stop();
    }


    /// <summary>
    /// When the entity is updated (Physics)
    /// </summary>
    void FixedUpdate()
	{
		if (!isLocalPlayer)
			return;

	}

	/// <summary>
	/// When the entity begins colliding with another entity
	/// </summary>
	/// <param name="collision">the collision object</param>
	void OnCollisionEnter2D(Collision2D collision)
	{
		if (!isLocalPlayer)
			return;

        Debug.Log(collision.gameObject.tag);

		switch( collision.gameObject.tag )
		{
		case TAG_PLAYER:
			OnCollideWithPlayer(collision);
			break;
		case TAG_HOLYGRAIL:
			OnCollideWithHolyGrail(collision);
			break;
		case TAG_PROPS:
			OnCollideWithProp(collision);
			break;
		case TAG_TERRAIN:
			OnCollideWithTerrain(collision);
			break;
        case TAG_POWERUP:
            OnCollideWithPowerup(collision);
            break;

        }


	}

    private void OnCollideWithPowerup(Collision2D collision)
    {
        Debug.Log("OnCollideWithPowerup");
    }

    /// <summary>
    /// When the entity begins colliding with a trigger entity
    /// </summary>
    /// <param name="collider">the collider object</param>
    void OnTriggerEnter2D(Collider2D collider)
	{
		if (!isLocalPlayer)
			return;

		switch (collider.gameObject.tag)
		{
		case TAG_PLAYER:
			OnTriggerWithPlayer(collider);
			break;
		case TAG_HOLYGRAIL:
			OnTriggerWithHolyGrail(collider);
			break;
		case TAG_PROPS:
			OnTriggerWithProp(collider);
			break;
		case TAG_TERRAIN:
			OnTriggerWithTerrain(collider);
			break;
        case TAG_POWERUP:
            OnTriggerWithPowerup(collider);
            break;
        }
	}
    
    private void OnTriggerWithPowerup(Collider2D collider)
	{

        Debug.Log("OnTriggerWithPowerup");

        String name = collider.gameObject.name;

        if (name.Contains("bomb"))
        {
            this.gameObject.GetComponent<GameState>().Powerup("bomb");
            StartCoroutine(gameObject.GetComponent<PlayerMovementBehaviour>().ShowMessage(BOMB_PICKUP_TAG, 3));
        }
        else if (name.Contains("stamina"))
        {
            gameObject.GetComponent<Stamina>().resetStamina();
            StartCoroutine(gameObject.GetComponent<PlayerMovementBehaviour>().ShowMessage(STAMINA_POWERUP_TAG, 3)); 
        }

        GetComponent<PlayerMovementBehaviour>().CmdPlayPowerupSound();
    }

    [Command]
    private void CmdDestroy(GameObject o)
    {
        Vector3 powerupPos = o.transform.position;
        NetworkServer.Destroy(o);
        GameObject.FindGameObjectWithTag("powerupspawner").GetComponent<PowerUpSpawner>().PickedUpPowerUp(powerupPos);
    }

    /// <summary>
    /// When the entity collides with a player
    /// </summary>
    /// <param name="collision"></param>
    void OnCollideWithPlayer(Collision2D collision)
	{
		Debug.Log("I collided with another player...");

		// did I collide with a holy grail owner?
		if ( holyGrailOwner == collision.gameObject.GetComponent<NetworkIdentity>().netId )
		{
			holyGrailOwner = this.netId;
		}
	}

	/// <summary>
	/// When the entity collides with the holy grail
	/// </summary>
	/// <param name="collision"></param>
	void OnCollideWithHolyGrail(Collision2D collision)
	{
		Debug.Log("I collided with the holy grail!");
	}

	/// <summary>
	/// When the entity collides with a map prop
	/// </summary>
	/// <param name="collision"></param>
	void OnCollideWithProp(Collision2D collision)
	{

	}

	/// <summary>
	/// When the entity collides with the terrain
	/// </summary>
	/// <param name="collision"></param>
	void OnCollideWithTerrain(Collision2D collision)
	{
		Debug.Log("Terrain");
	}

	/// <summary>
	/// When the entity triggers a player trigger object
	/// </summary>
	/// <param name="collider"></param>
	void OnTriggerWithPlayer(Collider2D collider)
	{
		Debug.Log("I triggered with a player!");
	}

	/// <summary>
	/// When the entity triggers a holy grail trigger object
	/// </summary>
	/// <param name="collider"></param>
	void OnTriggerWithHolyGrail(Collider2D collider)
	{
		Debug.Log("I triggered the holy grail!");

		// assign myself
		holyGrailOwner = this.netId;

		this.gameObject.GetComponent<GameState>().onCrownCatch();
		
        // destroy this component
		CmdDestroyCrown();

        // warn every player
        this.gameObject.GetComponent<PlayerMovementBehaviour>().CmdShowText("The Holy Grail has been picked up.", 3f);
	}

	[Command]
	void CmdDestroyCrown(){

        NetworkServer.Destroy(GameObject.FindGameObjectWithTag("holy_grail"));
        Debug.Log("Holy grail has been destroyed!!!!!");
	}

    [Command]
    public void CmdCreateCrown()
    {
        GameObject.FindGameObjectWithTag("crown_spawner").GetComponent<StartingCrownSpawn>().createCrown();
        Debug.Log("Holy grail has been created!!!!!");
    }

    /// <summary>
    /// When the entity triggers a prop trigger object
    /// </summary>
    /// <param name="collider"></param>
    void OnTriggerWithProp(Collider2D collider)
	{

	}

	/// <summary>
	/// When the entity triggers a terrain trigger object
	/// </summary>
	/// <param name="collider"></param>
	void OnTriggerWithTerrain(Collider2D collider)
	{

	}

}