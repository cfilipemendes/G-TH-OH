using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Impurity : NetworkBehaviour
{

    public RectTransform impurityBar;
    public const float IMPURITY_START_VALUE = 100f;

    [SyncVar(hook = "OnChangeImpurity")]
    public float currentImpurity = IMPURITY_START_VALUE;

    public void decreaseImpurity(float amount)
	{	
		
        if (!isServer)
            return;
		
		currentImpurity -= amount;
        

		if (currentImpurity <= 0)
        {
			
            Debug.Log("Ganhei o jogo !");
            //Fazer um command a indicar que ganhei o jogo
            //gameObject.GetComponent<PlayerNetworkLogic>().CmdCreateCrown();
            //gameObject.GetComponent<GameState>().RpcOnCrownLost(); // Se um jogador perdeu a crown, "todos" perderam a crown (metodo bacoco para sync)
			gameObject.GetComponent<GameState>().RpcSetGameEnd(true);

		}
    }

    void OnChangeImpurity(float impurity)
    {
        impurityBar.sizeDelta = new Vector2(impurity, impurityBar.sizeDelta.y);
    }


}

