using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// AnimationController controls all animation logic and transition
/// </summary>
public class AnimationController : MonoBehaviour
{
    public Animator animator;
    public PlatformerController controller;

    public static AnimationController AnimatorInstance;

    #region Conditions
    public bool onWall { get; set; }
    public bool slide { get; set; }
    private bool grounded;
    private bool walking;
    private bool jumping;
    private bool boosting;
    private bool dashing;
    private bool stopping;
    private bool performingaction;
    #endregion

    private Vector2 velocity;
    private int _currentState;  //current state of player


    #region AnimationHashes

    private static readonly int _idle = Animator.StringToHash("player_idle");
    private static readonly int _run = Animator.StringToHash("player_run");
    private static readonly int _stop = Animator.StringToHash("player_stop");
    private static readonly int _jump = Animator.StringToHash("player_jump");
    private static readonly int _fall = Animator.StringToHash("player_fall");
    private static readonly int _dash = Animator.StringToHash("player_dash");
    private static readonly int _boost = Animator.StringToHash("player_boost");
    private static readonly int _slide = Animator.StringToHash("player_slide");
    private static readonly int _wallmount = Animator.StringToHash("player_mount");

    #endregion  

    #region Begin
    private void OnValidate()
    {
        AnimatorInstance = this;
    }

    private void Awake()
    {
        AnimatorInstance = this;
    }

    private void Start()
    {
        _currentState = _idle;
    }
    #endregion


    private void OnEnable()
    {
        AdvanceJump.boost += onBoost;
        DashScript.dash_Event += onDash;
    }

    private void OnDisable()
    {
        AdvanceJump.boost -= onBoost;
        DashScript.dash_Event -= onDash;
    }

    void onBoost()
    {
        boosting = true;
    }

    void onDash()
    {
        dashing= true;
    }

    private void Update()
    {
        GetConditions();

        var _state = GetState();

        //Set State
        if (_state == _currentState) return;
        animator.CrossFade(_state, 0.25f, 0);
        _currentState = _state;
    }

    private void GetConditions()
    {
        performingaction = controller.performing_action;
        grounded = controller.isGrounded;

        velocity = PlatformerController.PlayerVelocity.normalized;
        walking = (Mathf.Abs(velocity.x) > 0.1f) ? true: false;  

        //when player is still moving but is not receiving input,player is trying to stop
        if(walking && Mathf.Abs(controller._input.x) == 0 && grounded) stopping= true; else stopping= false;

        if (onWall) { boosting = false; dashing = false; }

        if(grounded && !controller.performing_action)
        {
            boosting= false;
        }

        if (!performingaction)
        {
            dashing= false;
        }
    }

    private float lockTill;
    private int GetState()
    {
        if ((Time.time < lockTill)) return _currentState;

        if (slide) return _slide;

        if (onWall) return _wallmount;

        if (dashing) return _dash;

        if (boosting) return _boost;

        if (performingaction) return _currentState;

        if (stopping) return LockState(_stop, 0.1f);

        if (grounded) return (walking) ? _run : _idle;

        return (velocity.y > 0) ? _jump : _fall;

        int LockState(int state, float duration)
        {
            lockTill = Time.time + duration;
            return state;
        }

    }

}
