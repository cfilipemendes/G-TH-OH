using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Health : NetworkBehaviour
{

    public RectTransform HealthBar;

    public const float HEALTH_START_VALUE = 100f;

    [SyncVar(hook = "OnChangeHealth")]
    public float currentHealth;

	public void OnStart(){
		currentHealth = HEALTH_START_VALUE;
	}


	public void decreaseHealth(float amount)
    {
		if(!isServer)
			return;

            currentHealth -= amount;
		    Debug.Log (currentHealth);
       
		    if (currentHealth <= 0)
            {
                Debug.Log("Morri");
                //morre
                gameObject.GetComponent<PlayerNetworkLogic>().CmdCreateCrown();
                gameObject.GetComponent<GameState>().RpcOnCrownLost(); // Se um jogador perdeu a crown, "todos" perderam a crown (metodo bacoco para sync)
                
            }
    }

    void OnChangeHealth(float health)
    {
        HealthBar.sizeDelta = new Vector2(health, HealthBar.sizeDelta.y);
    }

    public void resetHealth()
    {
        currentHealth = HEALTH_START_VALUE;
    }
}

