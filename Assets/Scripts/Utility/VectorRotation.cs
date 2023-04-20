using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorRotation : MonoBehaviour
{
    public enum type { GetAngle,SetAngle,both};
    [SerializeField]private type VectorAction;

    [Header("GetAngle"),Space(10)]
    [SerializeField] private Transform target;
    [SerializeField] private float AngleBetween;

    [Header("SetAngle"),Space(10)]
    [SerializeField] private float RotateSpeed;
    


    // Update is called once per frame
    void FixedUpdate()
    {
        switch (VectorAction)
        {
            case type.GetAngle:
                GetAngle();
                break;
            case type.SetAngle:
                SetAngle();
                break;
            case type.both:
                GetAngle();
                SetAngle();
                break;
            default:
                break;
        }
    }
    private void GetAngle()
    {
        var a = transform.position - target.position;
        var angle = Vector3.Angle(-a.normalized, Vector2.right.normalized);
        var positive_angle = (Mathf.Sign(-a.normalized.y) > 0);
        AngleBetween = (positive_angle) ? angle : (360 - angle);
    }

    private void SetAngle()
    {
        var currentPosition = target.position;
        var rotatedPosition = Quaternion.AngleAxis(RotateSpeed, transform.forward) * currentPosition;
        target.position = rotatedPosition;
    }
}
