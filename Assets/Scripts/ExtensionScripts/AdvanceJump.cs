using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputHandeler;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController), typeof(StaminaScript))]
public class AdvanceJump : MonoBehaviour
{
    public Jump_Types jumpType = Jump_Types.NormalJump;

    #region components
    private Rigidbody2D m_rigidbody;
    private PlatformerController m_platformController;
    private StaminaScript stamina;
    #endregion

    [SerializeField]
    private float StaminaUsage = 10f;
    [Space(10)]

    [HideInInspector]public float a_bufferTime = 0.2f;
    [HideInInspector] public float[] force = new float[3]{ 7f,10f,3f};

    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_platformController = GetComponent<PlatformerController>();
        stamina= GetComponent<StaminaScript>();
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
                break;
            default:
                break;
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
            if (jumpIndex == 1 && m_platformController.lastOnGround==false && stamina.GetValue() >= StaminaUsage)
            {
                Jump();
                KeyEvents.KeysCalled.Remove("jump");
                stamina.changeValue(-StaminaUsage);
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
