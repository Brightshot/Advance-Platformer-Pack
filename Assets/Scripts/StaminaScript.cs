using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaScript : MonoBehaviour
{
    public Image staminaBar;

    private PlatformerController controller;

    private bool recharging,startRecharge=false;
    public float maxStamina;
    [SerializeField]
    private float rechargeDelay;
    [SerializeField]
    private float rechargeSpeed=0.1f;
    private float m_stamina {get;set;}

    private void Start()
    {
        m_stamina = maxStamina;
        controller = GetComponent<PlatformerController>();
    }
    
    private void Update()
    {
        m_stamina = Mathf.Clamp(m_stamina, 0, maxStamina);

        staminaBar.fillAmount = m_stamina / maxStamina;

        if (!controller.performing_action && !startRecharge)
        {
            startRecharge= true;
            StartCoroutine("Recharge");
        }

        if(controller.performing_action && startRecharge)
        {
            startRecharge = false;
            recharging = false;
            StopCoroutine("Recharge");
        }

        if (recharging)
        {
            changeValue(rechargeSpeed/2);
        }
    }

    public float GetValue()
    {
        return m_stamina;
    }

    public void reset()
    {
        startRecharge = false;
        recharging = false;
        StopCoroutine("Recharge");
    }

    public void changeValue(float value)
    {
        m_stamina += value;
    }

    IEnumerator Recharge()
    {
        yield return new WaitForSeconds(rechargeDelay);
        recharging= true;
    }
}
