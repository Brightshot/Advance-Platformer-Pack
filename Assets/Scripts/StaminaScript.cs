using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaScript : MonoBehaviour
{
    public Image staminaBar;

    private bool recharging;
    public float maxStamina;
    [SerializeField]
    private float rechargeDelay;
    private float m_stamina {get;set;}

    private void Start()
    {
        m_stamina = maxStamina;
    }
    
    private void Update()
    {
        m_stamina = Mathf.Clamp(m_stamina, 0, maxStamina);
        staminaBar.fillAmount = m_stamina / maxStamina;

        if (recharging)
        {
            changeValue(maxStamina/60);
        }
    }

    public float GetValue()
    {
        return m_stamina;
    }

    public void changeValue(float value)
    {
        m_stamina += value;
        recharging = false;
    }

    IEnumerator Recharge()
    {
        yield return new WaitForSeconds(rechargeDelay);
        recharging= true;
    }
}
