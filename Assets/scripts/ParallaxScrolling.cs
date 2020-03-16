using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParallaxScrolling : MonoBehaviour {

    public Transform[] backgrounds;
    private float[] parallaxScales;
    public float smoothing = 1f;

    private Transform cameraPosition;
    private Vector3 previousCameraPosition;

    /// <summary>
    /// 
    /// </summary>
	void Awake()
    {
        cameraPosition = Camera.main.transform;
    }

    /// <summary>
    /// 
    /// </summary>
	void Start ()
    {
        
        previousCameraPosition = cameraPosition.position;
        parallaxScales = new float[backgrounds.Length];
        for( int i = 0; i < parallaxScales.Length; i++ )
        {
            parallaxScales[i] = backgrounds[i].position.z * -1f;
        }
	}
	
	/// <summary>
    /// 
    /// </summary>
	void Update ()
    {
		Scene actualScene = SceneManager.GetActiveScene();

		// ADICIONAR AQUI AS CENAS DOS CIRCULOS (FALTA A DO VOLCANO/HELL)
		if (!(actualScene.name.Equals ("circle_limbo") || actualScene.name.Equals ("circle_vulcano")))
			return;
		
        for( int i = 0; i < backgrounds.Length; i++ )
        {
            float parallax = (previousCameraPosition.x - cameraPosition.position.x) * parallaxScales[i];
            Vector3 backgroundPos = new Vector3(backgrounds[i].position.x + parallax, backgrounds[i].position.y, backgrounds[i].position.z);
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundPos, smoothing * Time.deltaTime);
        }

        previousCameraPosition = cameraPosition.position;
	}
}
