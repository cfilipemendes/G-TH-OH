using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public class Stamina : NetworkBehaviour
{

    public RectTransform StaminaBar;

    public RectTransform barraToda;

    public const float STAMINA_START_VALUE = 300f;

    public float currentStamina = STAMINA_START_VALUE;

    public const float STAMINA_REGEN_RATE = 25f;


    private bool decreasing = false;
    private float decreaseOverTime = 0.0f;

    void Update()
    {
        if (!decreasing)
        {
            if ((currentStamina + STAMINA_REGEN_RATE * Time.deltaTime) <= STAMINA_START_VALUE)
            {
                currentStamina += STAMINA_REGEN_RATE * Time.deltaTime;
                StaminaBar.sizeDelta = new Vector2(currentStamina, StaminaBar.sizeDelta.y);
            }
        }
        else
        {
            if (decreaseOverTime * Time.deltaTime <= currentStamina)
            {
                currentStamina -= decreaseOverTime * Time.deltaTime;
                StaminaBar.sizeDelta = new Vector2(currentStamina, StaminaBar.sizeDelta.y);
            }
            else
            {
                stopDecreasingStamina();
            }
        }
    }

    public bool decreaseStamina(float amount)
    {

        if (amount <= currentStamina)
        {
            currentStamina -= amount;
            StaminaBar.sizeDelta = new Vector2(currentStamina, StaminaBar.sizeDelta.y);
            return true;
        }
        else
            return false;
    }

    public bool decreaseStaminaOverTime(float amount)
    {
        if ((amount * Time.deltaTime) <= currentStamina)
        {
            decreasing = true;
            decreaseOverTime = amount;
            return true;
        }
        else
        {
            stopDecreasingStamina();
            return false;
        }  
    }

    public void stopDecreasingStamina()
    {
        decreasing = false;
        decreaseOverTime = 0.0f;
    }

    public void resetStamina()
    {
        currentStamina = STAMINA_START_VALUE;
        StaminaBar.sizeDelta = new Vector2(currentStamina, StaminaBar.sizeDelta.y);
    }

    public override void OnStartLocalPlayer()
    {
        barraToda.gameObject.SetActive(true);
    }

}

