using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PurificationBarsLayout : NetworkBehaviour {

    [SyncVar]
    public GameObject purificationBar;

    [SyncVar]
    public GameObject nicknamePlaceholder;

    private int ligacoes;
    private string playerName;

	// Use this for initialization
	void Start () {

        if (!isLocalPlayer)
            return;
        CmdQtasLigacoes();

        playerName = PlayerPrefs.GetString("nickname");
        
    }

    void Update()
    {
        CmdSetMyNickname(playerName);
    }

    [Command]
    void CmdQtasLigacoes()
    {
        RpcQtasLigacoes(NetworkServer.connections.Count);
    }

    [ClientRpc]
    void RpcQtasLigacoes(int lig)
    {  
        Debug.Log("Ligacoes:" + lig);
         
        //float x = purificationBar.GetComponent<RectTransform>().localPosition.x;
        float x = purificationBar.GetComponent<RectTransform>().anchoredPosition.x;
        //float y = purificationBar.GetComponent<RectTransform>().localPosition.y;
        float y = purificationBar.GetComponent<RectTransform>().anchoredPosition.y;

        
        purificationBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(x + (Screen.width/5) *(lig-1), y);


        float nx = nicknamePlaceholder.GetComponent<RectTransform>().anchoredPosition.x;
        float ny = nicknamePlaceholder.GetComponent<RectTransform>().anchoredPosition.y;
        nicknamePlaceholder.GetComponent<RectTransform>().anchoredPosition = new Vector2(nx + (Screen.width / 5) * (lig - 1), ny);
        
    }

    [Command]
    public void CmdSetMyNickname(string nick)
    {
        RpcSetMyNickname(nick);
    }

    [ClientRpc]
    void RpcSetMyNickname(string nick)
    {
        nicknamePlaceholder.GetComponent<Text>().text = nick;
    }

}
