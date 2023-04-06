using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController), typeof(StaminaScript))]
public class WallClimb : MonoBehaviour
{
    public LayerMask WallMask;
    public Vector2 scanArea = new Vector2(.1f,.8f);
    public Vector2 offset = new Vector2(.5f,0f);

    [SerializeField] private float Force = 10f;
    [SerializeField] private float staminaUsage = .1f;

    private PlatformerController controller;
    private StaminaScript stamina;
    private Rigidbody2D m_rigidbody;
    private int dir=1;

    private void Start()
    {
        controller = GetComponent<PlatformerController>();
        m_rigidbody= GetComponent<Rigidbody2D>();
        stamina = GetComponent<StaminaScript>();
    }

    private bool onWall;
    bool re=false;
    private void Update()
    {
        if(!onWall)
        dir = (int)controller.Visuals.localScale.x;

        //Check if the player is still on ground or close
        var ray = Physics2D.Raycast(transform.position,-transform.up, 2);

        onWall = Physics2D.OverlapBox((Vector2)transform.position + (offset * dir), scanArea, 90f) && ray.collider==null;

        if (onWall && stamina.GetValue() >= staminaUsage)
        {
            re = false;
            m_rigidbody.drag = 15;
            controller.performing_action = true;
            stamina.changeValue(-staminaUsage);

            if(onWall && Input.GetKeyDown(KeyCode.Space))
            {
                m_rigidbody.velocity = Vector2.zero;
                m_rigidbody.AddForce(transform.up * Force,ForceMode2D.Impulse);
                m_rigidbody.AddForce(-transform.right * Mathf.Sign(controller.Visuals.localScale.x) * Force, ForceMode2D.Impulse);
                controller.performing_action = false;
                controller.SetDirection(-(int)controller.Visuals.localScale.x);
                onWall = false;
            }
        }
        else
        { 
            m_rigidbody.drag = .5f;
            if (ray.collider != null && !re)
            {
                controller.performing_action = false;
                onWall = false;
                re = true;
            }
        }
    }

    private void Wall_Climb()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + (offset*dir), scanArea);
    }

}
