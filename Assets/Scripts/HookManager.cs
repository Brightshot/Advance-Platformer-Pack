using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

public class HookManager : MonoBehaviour
{
    /// <summary>
    /// Selection by distance~
    ///     Pressing left or right arrow allow switching select object from closest to furthest,Good for snappy response
    ///     but not as quick as "DotSelect"
    ///     
    /// Selection by DotProduct~
    ///     Takes a look direction from player and selects the object the player is most likely looking at
    ///     (can work with mouse or joystick input),quick and precise 
    /// </summary>
    public enum SelectType { DistanceSelect,DotSelect}
    public SelectType selectType { get; set; } = SelectType.DistanceSelect;

    public static HookManager instance;
    public List<Transform> grappleHooks { get; private set; } = new List<Transform>();

    public Transform selected { get; private set; }
    public Sprite sprite { get; set; }
    public Transform selectCircle { get; set; }
    public float searchDistance { get; set; } = 20;

    [Space(20)]
    #region Rope settings

    public LineRenderer lineRenderer;
    [SerializeField] private int resolution = 40;

    private float time;

    [Header("Rope Settings")]
    public float Firespeed = 4;
    [SerializeField] private float RetractSpeed = 7;
    [SerializeField] private float amplitude = .4f;
    [SerializeField] private float frequency = 2;
    [SerializeField] private float smoothDistance = 0.8f;
    public bool connected { get;  set; }
    public Transform player { get; set; }

    private AnimationCurve ropeProgression;
    public AnimationCurve ropeRetract = AnimationCurve.Linear(0, 0, 1, 1);
    #endregion 

    private void Awake()
    {
        instance = this;
    }

    private void OnValidate()
    {
        instance = this;
    }

    private void Start()
    {
        var circle = new GameObject("SelectCircle").AddComponent<SpriteRenderer>();
        circle.sprite = sprite;
        selectCircle = circle.transform;
        selectCircle.gameObject.SetActive(false);

        //Rope
        ropeProgression = AnimationCurve.Linear(0, 1f, smoothDistance, 0);
    }

    private void Update()
    {
        if(selected!=null) selectCircle.position = selected.position;
        RopeLogic();
    }

    private void FixedUpdate()
    {
        DrawWaves();
    }

    private void RopeLogic()
    {
        lineRenderer.transform.parent = null;

        ropeProgression = AnimationCurve.Linear(0, 1f, smoothDistance, 0);

        float speed = (connected) ? Firespeed : RetractSpeed;

        if (connected)
        {
            time += Time.deltaTime * speed;
        }
        else
        {
            time -= Time.deltaTime * speed;
        }
        time = Mathf.Clamp(time, 0, 1);
    }


    //Draw waves to grapple hook wave
    Vector2 target;
    void DrawWaves()
    {
        lineRenderer.positionCount = resolution;

        if (player == null) return;

        if (selected != null)
        {  target = selected.position; }

        var localPosition = -(player.position - (Vector3)target);

        for (int i = 0; i < resolution; i++)
        {
            float delta = (float)i / (resolution - 1);

            var multiplier = ropeProgression.Evaluate(time);
            var retract = ropeRetract.Evaluate(time);

            var offset = Vector2.Perpendicular(localPosition).normalized;
            var targetPos = Vector2.Lerp(Vector2.zero, localPosition * retract, delta);

            float dx = Mathf.Lerp(0, localPosition.magnitude, delta) * retract;
            float dy = Mathf.Sin(dx * frequency) * amplitude;

            var currentPos = (new Vector2(targetPos.x, targetPos.y)) + (offset * dy * multiplier);

            lineRenderer.SetPosition(i, currentPos + (Vector2)player.position);
        }
    }


    #region Distance Selection

    public void NextonList()
    {
        var index = grappleHooks.IndexOf(selected) + 1;
        index = Mathf.Clamp(index, 0, grappleHooks.Count - 1);
        selected = grappleHooks[index];
    }

    public void PreviousOnList()
    {
        var index = grappleHooks.IndexOf(selected) - 1;
        index = Mathf.Clamp(index, 0, grappleHooks.Count - 1);
        selected = grappleHooks[index];
    }

    public void reset()
    {
        grappleHooks.Clear();
        selected = null;
    }

    //Can this search ,if yes then get the selected hook
    public bool CanSearch()
    {
        grappleHooks.Clear();

        //Get all hooks in scene and add hooks to list "grappleHooks" if its distance is within search distance
        grappleHooks = GameObject.FindGameObjectsWithTag("Hook").Select(t =>t.transform)
        .Where(t=> Vector2.Distance(t.position,player.position) <= searchDistance).ToList();

        if (grappleHooks.Count <= 0) return false;

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        //sort data by closest distance to player
        var orderedData = grappleHooks.OrderByDescending(t => player.position.x - (t.position.x)).ToList();
        grappleHooks = orderedData;

        //Select closest hook in list
        float distanceY = Mathf.Infinity;
        Transform select = null;

        foreach (var hook in grappleHooks)
        {
            var diff = Vector2.Distance(player.position, hook.position);
            if (diff < distanceY)
            {
                distanceY = diff;
                select = hook;
            }
        }

        if (selectType == SelectType.DistanceSelect)
        {
            selected = select;
            selectCircle.position = selected.position;
            selectCircle.gameObject.SetActive(true);
        }

        return true;
    }


    //Get ray direction and select object in the direction
    //used for dot select
    public void GetRay()
    {
        Vector2 dir = player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Transform BestTarget = null;
        float distance = -1;

        if (grappleHooks.Count <= 0) return;

        foreach (var hooks in grappleHooks)
        {
            var hookDirecction = player.position - hooks.position;
            var dotProduct = Vector2.Dot(hookDirecction.normalized, dir.normalized);
            /*Debug.Log($" Name : {hooks.name} , DotProduct : {dotProduct}");*/
            if (dotProduct >= 0.2f)
            {
                if (dotProduct > distance)
                {
                    distance = dotProduct;
                    BestTarget = hooks;
                }
            }
        }
        selected = BestTarget;
        selectCircle.position = selected.position;
        selectCircle.gameObject.SetActive(true);
    }
    #endregion
}
