using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static InputHandeler;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController), typeof(StaminaScript))]
public class AdvanceJump : MonoBehaviour
{
    public Jump_Types jumpType = Jump_Types.NormalJump;

    public delegate void ExtendJump();
    public ExtendJump extend_Jump;

    [SerializeField] private float staminaUsage;

    #region components
    private Rigidbody2D m_rigidbody;
    private PlatformerController m_platformController;
    private StaminaScript stamina;
    #endregion

    [Space(10)]

    [HideInInspector]public float a_bufferTime = 0.2f;
    [HideInInspector] public float[] force = new float[3]{ 7f,10f,3f};
    private float _time;
    public bool isJumping { get; private set; }

    public delegate void Boost();
    public static Boost boost;

    public static void animateBoost()
    {
        boost?.Invoke();
    }

    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_platformController = GetComponent<PlatformerController>();
        stamina = GetComponent<StaminaScript>();
    }

    private void Update()
    {
        if (m_platformController.performing_action == true) return;

        switch (jumpType)
        {
            case Jump_Types.NormalJump:
                NormalJump();
                break;
            case Jump_Types.DoubleJump:
                DoubleJump();
                break;
            case Jump_Types.ExtendedJump:
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    isJumping = false;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if(jumpType== Jump_Types.ExtendedJump)
        {
            ExtendedJump();
        }
    }

    AnimationCurve jumpFallOff;
    private void ExtendedJump()
    {
        jumpFallOff = AnimationCurve.Linear(0, 1, force[2],0);

        var fallOff = jumpFallOff.Evaluate(_time * 2f);

        if (!isJumping)
        {
            _time = 0 ;
        }

        if (_time >= force[2]) { isJumping = false; return; }

        if (Input.GetKey(KeyCode.Space) && m_platformController.lastOnGround)
        {
            isJumping= true;
        }
        
        if (Input.GetKey(KeyCode.Space) && isJumping && stamina.GetValue() >= staminaUsage)
        {
            _time += Time.deltaTime;
            m_rigidbody.AddForce(transform.up * 80 * fallOff, ForceMode2D.Force);

            animateBoost();
            stamina.changeValue(-staminaUsage);
        }
    }

    private void DoubleJump()
    {
        jumpIndex = Mathf.Clamp(jumpIndex, 0, 2);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CallKey("jump", a_bufferTime);
        }

        if (KeyEvents.pressed("jump"))
        {
            if (jumpIndex == 1 && m_platformController.lastOnGround==false && stamina.GetValue() >= staminaUsage)
            {
                Jump();
                animateBoost();
                KeyEvents.KeysCalled.Remove("jump");
                stamina.changeValue(-staminaUsage);
            }

            if (m_platformController.lastOnGround == true)
            {
                Jump();
                KeyEvents.KeysCalled.Remove("jump");
            }
        }

        if (m_platformController.lastOnGround == true)
        {
            jumpIndex = 0;
        }
    }

    private bool jumped;
    IEnumerator ResetJump()
    {
        jumped = true;
        yield return new WaitForSeconds(0.3f);
        jumped = false;
    }


    private void NormalJump()
    {
        jumpIndex = 0;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CallKey("jump", a_bufferTime);
        }


        if (m_platformController.lastOnGround == true && KeyEvents.pressed("jump"))
        {
            Jump();
            KeyEvents.KeysCalled.Remove("jump");
        }
    }

    private void CallKey(string key, float delay)
    {
        StartCoroutine(KeyEvents.KeyPressed(key, delay));
    }

    private int jumpIndex;
    public void Jump()
    {
        m_platformController.StartCoroutine("ResetJump");
        m_rigidbody.velocity = new Vector2(m_rigidbody.velocity.x, 0);
        m_rigidbody.AddForce(transform.up.normalized * force[jumpIndex], ForceMode2D.Impulse);
        m_platformController.lastOnGround = false;
        jumpIndex ++;
    }
}

public enum Jump_Types { NormalJump,DoubleJump,ExtendedJump };
