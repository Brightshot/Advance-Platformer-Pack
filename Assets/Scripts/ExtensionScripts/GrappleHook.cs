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

    [Header("KeyMapping")]
    [SerializeField] private KeyCode SearchKey = KeyCode.X;

    private bool connected;
    private bool rotating;
    private bool clockwise = false;
    bool isReady = true;

    private Transform target;
    private EffectsScripts vfx;
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

        if(manager.selectCircle!=null) manager.selectCircle.GetComponent<SpriteRenderer>().sprite= SelectionSprite;
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
        _time = HoldTime;
        vfx = EffectsScripts.Instance;
    }

    private void Update()
    {
        if(manager.selected!=null) target = manager.selected;

        if(stamina.GetValue() >= StaminaUsage)
        SelectionLogic();
    }

    private void LateUpdate()
    {
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
        if (Input.GetKey(SearchKey) && connected && _time > 0.1f)
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
        manager.reset();
        target = null;
        connected = false;
        //

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        manager.selectCircle.gameObject.SetActive(false);
        StartCoroutine("reset");

        if (rotating) { StartCoroutine("reset_action"); } else { controller.performing_action = false; rotating = false; }
    }

    private void SelectionLogic()
    {
        if (Input.GetKeyDown(SearchKey) && isReady && !connected && !controller.performing_action)
        {
             StopCoroutine("reset");

             manager.Search(transform);

             //grapple hook is activated player is performing an action
             controller.performing_action = true;

        }

        if (selectionType == HookManager.SelectType.DotSelect)
        {
            if (Input.GetKey(SearchKey) && isReady && !connected)
            {
                manager.GetRay(transform);
            }
        }

        #region TimeHandle

        if (Input.GetKey(SearchKey) && isReady && !connected)
        {
            _time -= Time.deltaTime;
            _time = Mathf.Clamp(_time,0,Mathf.Infinity);
        }


        if (Input.GetKeyUp(SearchKey)) _time = HoldTime;
        #endregion

        if (target == null) return;

        if (_time <= 0 && !connected)
        {
            ShakeController.Shake();
            connected = true;
            isReady = false;
        }

        if (Input.GetKey(SearchKey) && _time > 0.1f && !connected)
        {
            vfx.LensValue = -.3f;
            vfx.Target = target;
        }
        else
        {
            vfx.Target = null;
            vfx.LensValue = 0;
        }

        //connect to hook when key released
        if (Input.GetKeyUp(SearchKey) && _time > 0.1f)
        {
            ShakeController.Shake();
            stamina.changeValue(-5);
            connected = true;
            isReady = false;
        }

        if (!rotating)
        {
            var direction = transform.position - target.position;
            if (Mathf.Sign(direction.x) == 1) clockwise = true; else if (Mathf.Sign(direction.x) == -1) clockwise = false;
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
        tangent = Quaternion.AngleAxis(90, transform.forward) * dir;
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

    #endregion
}
