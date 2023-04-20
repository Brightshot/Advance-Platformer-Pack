using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRope : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform target;
    [SerializeField]private int resolution = 40;

    private float time;

    [Header("Rope Settings")]
    [SerializeField] private float Firespeed=4;
    [SerializeField] private float RetractSpeed=7;
    [SerializeField] private float amplitude=.4f;
    [SerializeField] private float frequency=2;
    [SerializeField] private float smoothDistance = 0.8f;

    private bool hook;

    private AnimationCurve ropeProgression;
    public AnimationCurve ropeRetract = AnimationCurve.Linear(0, 0, 1, 1);

    void Update()
    {
        RopeLogic();
    }

    private void RopeLogic()
    {
        hook = (Input.GetKey(KeyCode.G)) ? true : false;

        lineRenderer.transform.parent = null;

        ropeProgression = AnimationCurve.Linear(0, 1f, smoothDistance, 0);

        float speed = (hook) ? Firespeed : RetractSpeed;

        if (hook)
        {
            time += Time.deltaTime * speed;
        }
        else
        {
            time -= Time.deltaTime * speed;
        }
        time = Mathf.Clamp(time, 0, 1);
    }

    private void FixedUpdate()
    {
        DrawWaves();
    }

    void DrawWaves()
    {
        lineRenderer.positionCount = resolution;

        var localPosition = -(transform.position - target.position);

        for (int i = 0; i < resolution; i++)
        {
            float delta = (float)i / (resolution - 1);

            var multiplier = ropeProgression.Evaluate(time);
            var retract = ropeRetract.Evaluate(time);

            var offset = Vector2.Perpendicular(localPosition).normalized;
            var targetPos = Vector2.Lerp(Vector2.zero, localPosition*retract, delta);

            float dx = Mathf.Lerp(0,localPosition.magnitude,delta) * retract;
            float dy = Mathf.Sin(dx*frequency) * amplitude;

            var currentPos = (new Vector2(targetPos.x, targetPos.y)) + (offset * dy * multiplier) ;

            lineRenderer.SetPosition(i,currentPos + (Vector2)transform.position);
        }
    }
}
