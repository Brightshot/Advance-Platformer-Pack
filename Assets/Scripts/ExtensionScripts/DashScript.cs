using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D),typeof(PlatformerController),typeof(StaminaScript))]
public class DashScript : MonoBehaviour
{
    #region Components
    private Rigidbody2D m_Rigidbody;
    private PlatformerController controller;
    private StaminaScript stamina;
    #endregion

    public GameObject DashParticle;

    private bool has_dashed;
    [SerializeField]
    private float StaminaUsage = 100f;
    [Space(10)]

    [SerializeField] private float DashForce=10;
    [SerializeField] private float StartDelay=0;
    [SerializeField] private float RechargeDelay = 4;

    [Space(10),SerializeField]private KeyCode DashKey = KeyCode.Z;

    public delegate void DashEvent();
    public static DashEvent dash_Event;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlatformerController>();
        stamina = GetComponent<StaminaScript>();
    }

    private void Update()
    {
        //Start dash 
        if(Input.GetKeyDown(DashKey) && !has_dashed && stamina.GetValue() >= StaminaUsage)
        {
            if (controller.performing_action == false)
            {
                if(DashParticle!=null)Instantiate(DashParticle, transform.position, Quaternion.identity);
                StartCoroutine("StartDash");
            }
        }
    }

    #region numerators
    private IEnumerator resetDash()
    {
        yield return new WaitForSeconds(.5f);

        controller.performing_action = false;

        yield return new WaitForSeconds(RechargeDelay);
        has_dashed = false;
    }

    //Initiate dash after delay
    private IEnumerator StartDash()
    {
        controller.performing_action = true;
        m_Rigidbody.simulated = false;
        yield return new WaitForSeconds(StartDelay);
        m_Rigidbody.simulated = true;

        dash_Event?.Invoke();   //Invoke dash
        Dash();
    }
    #endregion

    //Dash logic
    private void Dash()
    {
        has_dashed= true;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.AddForce((transform.right * controller.Visuals.localScale.x) * DashForce * (Time.fixedDeltaTime*100),ForceMode2D.Impulse);

        //use 'staminaUsage ~ 100 ' stamina
        stamina.changeValue(-StaminaUsage);

        StartCoroutine("resetDash");
    }
}
