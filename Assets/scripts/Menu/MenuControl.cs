using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour {

	public void LoadScene(string sceneName)
	{
        if ( PlayerPrefs.GetString("nickname") != null )
		    SceneManager.LoadScene (sceneName);
        else
        {
            SetNickname("Anonymous");
            SceneManager.LoadScene(sceneName);
        }
	}

    public void SetNickname(string nickname)
    {
        PlayerPrefs.SetString("nickname", nickname);
    }
}
