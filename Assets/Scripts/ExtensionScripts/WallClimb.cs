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
    public Vector2 feetOffset = new Vector2(.0f,-.5f);

    [SerializeField] private float Force = 10f;
    [SerializeField] private float staminaUsage = .1f;
    [SerializeField] private float rayDistance = 2f;

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

    public bool onWall { get; private set; }
    bool reset=false,isReady=true;


    private void Update()
    {
        if(!onWall)
        dir = (int)controller.Visuals.localScale.x;

        AnimationController.AnimatorInstance.onWall = onWall;

        //Check if the player is still on ground or close
        var ray = Physics2D.Raycast(transform.position + (Vector3)feetOffset,-transform.up, rayDistance);


        onWall = Physics2D.OverlapBox((Vector2)transform.position + (new Vector2 { x = offset.x * dir, y = offset.y }), scanArea, 90f,WallMask) && ray.collider==null; 
        //

        if (onWall && stamina.GetValue() >= staminaUsage)
        {
            if (isReady)
            {
                isReady = false;
                reset = false;
                m_rigidbody.drag = 15;
                controller.performing_action = true;

                var dir = (int)controller.Visuals.localScale.x;
            }

            stamina.changeValue(-staminaUsage);
        }
        else
        { 
            m_rigidbody.drag = .5f;
            if (ray.collider != null && !reset)
            {
                controller.performing_action = false;
                onWall = false;
                reset = true;
                StartCoroutine("reset_Value");
            }
        }

        if (onWall && Input.GetKeyDown(KeyCode.Space))
        {
            m_rigidbody.velocity = Vector2.zero;
            m_rigidbody.AddForce(transform.up * Force, ForceMode2D.Impulse); //upward force
            m_rigidbody.AddForce(-transform.right * Mathf.Sign(controller.Visuals.localScale.x) * Force, ForceMode2D.Impulse);  //side force

            controller.performing_action = false;
            controller.SetDirection(-dir);
            onWall = false;
            StartCoroutine("reset_Value");
        }
    }

    IEnumerator reset_Value()
    {
        yield return new WaitForSeconds(.3f);
        isReady= true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + (new Vector2 { x = offset.x * dir, y = offset.y}), scanArea);
        Gizmos.DrawWireSphere(transform.position + (Vector3)feetOffset, .05f);
    }

}
