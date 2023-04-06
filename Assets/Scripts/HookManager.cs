using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    }

    private void Update()
    {
        if(selected!=null) selectCircle.position = selected.position;
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

    public void Search(Transform player)
    {
        grappleHooks.Clear();

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        grappleHooks = GameObject.FindGameObjectsWithTag("Hook").Select(t => t.transform).ToList();

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
    }

    public void GetRay(Transform player)
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
