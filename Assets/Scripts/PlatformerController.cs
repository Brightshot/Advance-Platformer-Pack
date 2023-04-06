using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static PlatformerController;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerController : MonoBehaviour
{

    #region Variables

    private Vector2 input;
    public static Vector2 PlayerVelocity { get; private set; }
    public Transform Visuals;
    [HideInInspector]public bool performing_action;

    #endregion

    #region Components

    protected Rigidbody2D m_Rigidbody;

    #endregion

    [Space]

    #region GroundCheckVariables

    [Header("Ground Check")]
    [SerializeField]private Vector2 detectionScale = new Vector2( 0.5f , 0.25f ) ;  //Scale of overlap box detecting ground
    [SerializeField]private Vector2 offset = new Vector2(0,-0.5f);  //overlap box offset from player position 

    [Space]

    public LayerMask GroundLayer;   //The Layers to be detected by overlap box
    [HideInInspector]public bool lastOnGround; //is player on ground after a time?

    public static bool isGrounded { get; private set; }
    [SerializeField, Space, Range(0, 1f)]
    private float cayoteTime = 0.2f;
    #endregion

    [Space]

    #region MovementVariables

    [Header("Movement")]
    [SerializeField]private float acceleration=8;
    [SerializeField]private float deceleration=6;
    [SerializeField]private float MaxSpeed=12;

    [Range(0,1f)]
    [SerializeField]private float velocityPower=0.3f;

    #endregion

    #region Events
    public delegate void M_Jump();
    public static M_Jump jump;
    #endregion

    [Space]

    #region JumpVariables

    [HideInInspector]public bool controlsJump=true;
    [HideInInspector]public float jumpForce=5;

    [Tooltip("The Amount of time the jump function should be true. \nThe Higher the value the further away from the ground jump can be called"),Range(0,0.6f)]
    [HideInInspector]public float bufferTime=0.2f;

    #endregion

    [Space]
    #region LedgeVariables
    [Header("LedgeBoost")]
    [SerializeField]private Vector2 ledgeRayOffset = new Vector2(0,-0.44f);
    [SerializeField]private float ledgeAdditionalForce = 1.5f;
    [SerializeField] private float ray_difference=0.35f;
    #endregion

    private void Start()
    {
        performing_action= false;
       m_Rigidbody = GetComponent<Rigidbody2D>();  //Rigidbody
    }

    private void Update()
    {
        Actions();
        InputHandle();
        CheckGround();
        LedgeBoost();

        if ((Mathf.Abs(input.x) > 0.05) && !performing_action)
        {
            Visuals.localScale = new Vector3(1 * Mathf.Sign(input.x), 1);
        }
    }

    public void SetDirection(int dir)
    {
        Visuals.localScale = new Vector3(1 * Mathf.Sign(dir), 1);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Actions()
    {
        //return if jump is not controlled by this script
        if (!controlsJump) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //create a keyevent called "jump" and call it
            CallKey("jump", bufferTime);
        }


        //if received keyevent "jump" and player is grounded
        if (lastOnGround == true && KeyEvents.pressed("jump"))
        {
            Jump();
            KeyEvents.KeysCalled.Remove("jump");
        }
    }

    public void CallJump()
    {
        CallKey("jump", bufferTime);
    }

    Vector2 aim;    //stores the scale of player when aiming
    private void InputHandle()
    {
        input = InputHandeler.InputAxis;
        PlayerVelocity = m_Rigidbody.velocity;
    }

    private void Movement()
    {
        if (performing_action) return;
        float targetSpeed = input.x * MaxSpeed;     //get the speed the player wants to move towards
        float speed_difference = targetSpeed - m_Rigidbody.velocity.x;
        float speed = (Mathf.Abs(input.x) > 0.1f) ? acceleration : deceleration;    //if moving forward accelerate else decelerate 
        float movementSpeed =  Mathf.Pow(Mathf.Abs(speed_difference) * speed,velocityPower) * Mathf.Sign(speed_difference);

        m_Rigidbody.AddForce(transform.right * movementSpeed * 10,ForceMode2D.Force);
    }

    private bool jumped;
    public void Jump()
    {
        StartCoroutine("ResetJump");
        jump?.Invoke();

        m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x , 0);     //reset Y velocity just before a jump to avoid inconsistent jump height.

        m_Rigidbody.AddForce(transform.up.normalized * jumpForce, ForceMode2D.Impulse);
        lastOnGround = false;
    }


    public IEnumerator ResetJump()
    {
        jumped = true;
        yield return new WaitForSeconds(0.3f);
        jumped = false;
    }

    //Ground Detection
    bool has_checked=false;
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapBox((Vector2)transform.position + offset, detectionScale, 90,GroundLayer);
        if (isGrounded && !jumped) lastOnGround = true;


        if (!isGrounded && !has_checked)
        {
            has_checked = true;
            StartCoroutine(CheckIfOnGround());
        }
        else if (isGrounded)
        {
            has_checked = false;
        }
    }

    /// <summary>
    /// Add an upward force when the player barely lands at the side of a ledge, 
    /// prevents the player from hitting a ledge and missing a jump by a few millimeters
    /// </summary>

    bool hasAided;
    private void LedgeBoost()
    {
        Vector2 dir = transform.right * Visuals.localScale.x;

        var pos = (Vector2)transform.position + ledgeRayOffset;
        var ray = Physics2D.Raycast(pos, dir, 1f);
        var ray2 = Physics2D.Raycast(pos+new Vector2(0,ray_difference), dir, 2f);

        bool rayDetected = (ray.collider != null && ray.collider != this.GetComponent<Collider2D>() && ray2.collider==null);
        bool rigidbodyConditions = m_Rigidbody.velocity.y > 0 && Mathf.Abs(m_Rigidbody.velocity.x) > 4;

        /*Debug.Log("ray1 : " + (ray.collider!=null) + " ray2 : " + (ray2.collider!=null) + " jumped: " + jumped);*/

        if (jumped && rayDetected &&  rigidbodyConditions && !hasAided)
        {
            StartCoroutine(Ledge_Boost());
            hasAided = true;
        }

        if (isGrounded)
        {
            hasAided = false;
        } 
    }

    #region Numerators

    private IEnumerator CheckIfOnGround()
    {
        yield return new WaitForSeconds(cayoteTime);
        lastOnGround = false;
    }

    //Boost upward
    private IEnumerator Ledge_Boost()
    {
        m_Rigidbody.AddForce(transform.up.normalized * ledgeAdditionalForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.05f);
        m_Rigidbody.AddForce(-transform.up.normalized * 2, ForceMode2D.Impulse);
    }

    #endregion

    #region EditorFunctions

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + offset, detectionScale);

        Gizmos.color = Color.blue;
        var pos = (Vector2)transform.position + ledgeRayOffset;

        Gizmos.DrawLine(pos, new Vector2(transform.position.x + (1f * Visuals.localScale.x), transform.position.y) + ledgeRayOffset);
        Gizmos.DrawLine(pos+new Vector2(0,ray_difference), new Vector2(pos.x + (2f * Visuals.localScale.x), pos.y) + new Vector2(0, ray_difference));
    }

    //Custom keyEvent system
    public void CallKey(string key,float delay)
    {
        StartCoroutine(KeyEvents.KeyPressed(key,delay));
    }
    #endregion
}