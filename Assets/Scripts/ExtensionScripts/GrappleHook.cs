using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController), typeof(StaminaScript))]
public class GrappleHook : MonoBehaviour,ISerializationCallbackReceiver
{
    [SerializeField] private HookManager.SelectType selectionType;

    [SerializeField]
    private Sprite SelectionSprite;
    [SerializeField] private float MaxSearchDistance = 20;

    [Header("KeyMapping")]
    [SerializeField] private KeyCode SearchKey = KeyCode.X;

    private bool connected;
    private bool rotating;
    private bool clockwise = false;
    bool isReady = true;
    bool pressed;

    private Transform target;
    private StaminaScript stamina;

    [Space(20)]
    [SerializeField,Range(0f,5f)] private float rotateSpeed=1.0f;

    [Tooltip("Note : A lower value makes the speed faster")]
    [SerializeField] private float movementSpeed=10;
    [SerializeField] private float releaseForce=20;

    [Space(10)]
    [SerializeField]
    private float StaminaUsage = 5f;

    [SerializeField] private float resetDelay = 0.2f;

    [SerializeField, Space(10)]
    private float HoldTime=2;
    private float _time;

    private PlatformerController controller;
    private HookManager manager;
    private EffectsScripts effects;

    public void OnBeforeSerialize()
    {
        manager = FindObjectOfType<HookManager>();
        if (manager == null)
        {
            manager = new GameObject("HookManager").AddComponent<HookManager>();
        }
        else
        {
            manager = HookManager.instance;
        }

        manager.selectType= selectionType;
        manager.sprite = SelectionSprite;
        manager.searchDistance = MaxSearchDistance;

        if (manager.selectCircle!=null) manager.selectCircle.GetComponent<SpriteRenderer>().sprite= SelectionSprite;
    }

    public void OnAfterDeserialize()
    {
        //Do nothing :)
    }

    private void Awake()
    {
        //Get manager at Awake
        manager = HookManager.instance;
        manager.sprite = SelectionSprite;
    }

    private void Start()
    {
        controller = GetComponent<PlatformerController>();
        stamina = GetComponent<StaminaScript>();
        manager.player = transform;
        _time = HoldTime;
        effects = EffectsScripts.Instance;
    }

    private void Update()
    {
        if(manager.selected!=null) target = manager.selected;

        //reset rotation when player disconnected
        if (!connected) controller.Visuals.up = Vector2.Lerp(controller.Visuals.up, Vector2.up, Time.deltaTime * 10);

        if (stamina.GetValue() >= StaminaUsage)
        SelectionLogic();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyUp(SearchKey)) pressed = false;

        if (Input.GetKeyUp(SearchKey) && connected && rotating)
        {
            Disconnect();

            //Add releaseForce
            var rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.velocity = Vector2.zero;

            var direction = (clockwise) ? -1 : 1;
            rigidbody.AddForce(tangent * releaseForce * direction, ForceMode2D.Impulse);

            rotating = false;
        }

        if (stamina.GetValue() <= 0 && connected)
        {
            Disconnect();

            //Add releaseForce
            var rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.velocity = Vector2.zero;

            var direction = (clockwise) ? -1 : 1;
            rigidbody.AddForce(tangent * releaseForce * direction, ForceMode2D.Impulse);

            rotating = false;
        }
    }

    private Vector2 velocity;
    private void FixedUpdate()
    {
        //rotate around pivot after search key is held after first release
        if (Input.GetKey(SearchKey) && connected && _time > 0.1f && !pressed)
        {
            rotating = true;
            SetAngle();
            stamina.changeValue(-StaminaUsage);
        }


        if (connected && !rotating)
        {
            var translation = Vector2.SmoothDamp(transform.position, target.position,ref velocity ,Time.fixedDeltaTime * (100 / movementSpeed));
            transform.position = translation;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (connected)
        Disconnect();
    }


    #region Methods

    private void Disconnect()
    {
        //reset 
        StopCoroutine("FireGrapple");
        manager.reset();
        target = null;
        connected = false;
        manager.connected = false;
        //~        

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        manager.selectCircle.gameObject.SetActive(false);
        StartCoroutine("reset");

        if (rotating) { StartCoroutine("reset_action"); } else { controller.performing_action = false; rotating = false; }
    }

    private void SelectionLogic()
    {
        if (Input.GetKeyDown(SearchKey) && isReady && !connected && !controller.performing_action)
        {

            if(!manager.CanSearch())
            {
                Disconnect();
                return;
            }
            else
            {
                controller.performing_action = true;
                effects.LensValue = .3f;
                effects.panel.color = Color.white;
                effects.isAnimating = true;
            }

            StopCoroutine("reset");
        }


        if (selectionType == HookManager.SelectType.DotSelect)
        {
            if (Input.GetKey(SearchKey) && isReady && !connected)
            {
                manager.GetRay();
            }
        }

        #region TimeHandle

        if (Input.GetKey(SearchKey) && isReady && !connected)
        {
            _time -= Time.unscaledDeltaTime;
            _time = Mathf.Clamp(_time,0,Mathf.Infinity);
        }
        #endregion

        if (target == null) return;

        int direction = (int)Mathf.Sign((transform.position - target.position).x) * 1;   //Get direction player visual

        if (connected)
            controller.Visuals.up = Vector2.Lerp(controller.Visuals.up,-(transform.position - target.position),Time.deltaTime * 5);
        else
            controller.SetDirection(-direction);

        //connect to hook when key released
        if (Input.GetKeyUp(SearchKey))
        {
            if(_time > 0.1f)
            StartCoroutine("FireGrapple");
        }

        if (_time <= 0 && !connected)
        {
            StartCoroutine("FireGrapple");
        }

        if (!rotating)
        {
            if (Mathf.Sign(direction) == 1) clockwise = true; else if (Mathf.Sign(direction) == -1) clockwise = false;
        }

        if (selectionType == HookManager.SelectType.DistanceSelect)
        {

            //Next on list
            if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && !connected)
            {
                manager.NextonList();
            }

            //previous on list
            if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && !connected)
            {
                manager.PreviousOnList();
            }
        }
       
        //Disconnect from hook at close range
        if (Vector2.Distance(transform.position, target.position) <= 1f)
        {
            Disconnect();
        }
    }

    /// <summary>
    /// ~rotate player either clock-wise or anti clock-wise 
    /// 
    /// ~get tangent of player when rotating
    /// </summary>
    Vector2 tangent;
    private void SetAngle()
    {
        //Set Rotation
        var direction = (clockwise) ? -1 : 1;
        var localposition = transform.position - target.position;
        var dist = Vector2.Distance(transform.position, target.position);
        dist = Mathf.Clamp(dist, 1f, Mathf.Infinity);
        var speed = rotateSpeed / Mathf.Pow(dist,0.5f);
        var rotatedPosition = Quaternion.AngleAxis(speed * direction * 10 , transform.forward) * localposition;
        transform.position = target.position + rotatedPosition;

        //Get Tangent
        var dir = (transform.position - target.position).normalized;   //normal direction
        tangent = Vector2.Perpendicular(dir);
    }     

    IEnumerator reset()
    {
        yield return new WaitWhile(() => Input.GetKey(SearchKey));

        yield return new WaitForSeconds(resetDelay);
        isReady = true;
    }

    IEnumerator reset_action()
    {
        rotating= false;
        yield return new WaitForSeconds(0.1f);
        controller.performing_action = false;
    }

    IEnumerator FireGrapple()
    {
        pressed = true;
        manager.connected = true;
        effects.LensValue = 0.0f;
        effects.isAnimating = false;
        _time = HoldTime;

        yield return new WaitForSeconds(1 / manager.Firespeed);
        
        isReady = false;
        ShakeController.Shake();
        stamina.changeValue(-5);
        connected = true;
    }

    #endregion
}
