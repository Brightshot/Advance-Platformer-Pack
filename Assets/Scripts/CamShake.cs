using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    private Vector3 offset_before_shake;
    private Transform pivot_target;

    private bool isShaking;

    [SerializeField] 
    private float amplitude=0.1f;
    private float current_amp;

    [SerializeField]
    private float frequency=1;
    private float displacement;
    private Vector2 startPosition;

    private int complete_displacement;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isShaking)
        {   
            var shake_displacement = new Vector3(offset_before_shake.x + (displacement),offset_before_shake.y + (displacement), -10f);

            //set position to player position displaced by offset
            if (pivot_target != null) 
                transform.position = pivot_target.position + shake_displacement; 
            else
                transform.position = (Vector3)startPosition + shake_displacement;
        }

        if(!revolution) current_amp = amplitude;

        //displace 
        if (revolution)
        {
            displacement = Mathf.MoveTowards(displacement,current_amp * direction,Time.deltaTime * frequency);
            if(displacement == (current_amp * Mathf.Sign(direction)))
            {
                complete_displacement++;

                //invert direction of shake after one completed displacement
                if (direction == 1) { direction = -1; } else if (direction == -1) { direction = 1; }

                if (complete_displacement < 2) return;

                revolution = false;
                StartCoroutine("InitiateShake");

                complete_displacement = 0;
            }
        }
    }

    [ContextMenu("Get Offset")]
    private void GetOffset()
    {
        var offset = transform.position - pivot_target.position;
        Debug.Log(offset);
    }

    public void Shake()
    {
        if (pivot_target != null)
        {
            offset_before_shake.z = -10;
            offset_before_shake = transform.position - pivot_target.position;
        }
        else
            offset_before_shake = Vector3.zero;

        isShaking = true;
        StartCoroutine("InitiateShake");
    }

    public void Stop()
    {
        current_amp = 0;
        StartCoroutine(StopShake());
    }

    #region numerators

    private bool revolution=false;
    private int direction=1;
    private IEnumerator InitiateShake()
    {
        yield return new WaitUntil(()=>revolution==false);

        revolution = true;

        yield return null;
    }

    private IEnumerator StopShake()
    {
        yield return new WaitUntil(()=> revolution==false);
        StopCoroutine("InitiateShake");
        isShaking= false;
        revolution= false;
    }

    #endregion
}
