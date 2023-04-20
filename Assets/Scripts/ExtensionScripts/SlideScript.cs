using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController), typeof(StaminaScript))]
public class SlideScript : MonoBehaviour
{

    #region components
    public BoxCollider2D _collider;
    private Rigidbody2D m_RigidBody;
    private PlatformerController controller;
    private StaminaScript staminaScript;
    #endregion

    [SerializeField] private float staminaUsage = 5f;

    [Space(10)]
    [SerializeField] private ColliderData normalColliderScale;
    [SerializeField] private ColliderData slideColliderScale;

    [Space(10),SerializeField] private float slideForce;

    private void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlatformerController>();
        staminaScript = GetComponent<StaminaScript>();
    }

    bool hasSlide;
    private void Update()
    {
        bool input = (Mathf.Abs(PlatformerController.PlayerVelocity.normalized.x) > 0.3f  && controller._input.y < -.5f );

        if (input && !hasSlide && controller.isGrounded && staminaScript.GetValue() >= staminaUsage)
        {
            Slide();
            hasSlide = true;
        }

        if(controller._input.y == 0)
        {
            hasSlide = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.performing_action = false;
            AnimationController.AnimatorInstance.slide = false;

            //reset collider
            _collider.size = normalColliderScale.size;
            _collider.offset = normalColliderScale.offset;
        }
    }

    private void Slide()
    {
        controller.performing_action = true;

        _collider.size = slideColliderScale.size;
        _collider.offset = slideColliderScale.offset;

        var direction = controller.Visuals.localScale.x;

        staminaScript.changeValue(-staminaUsage);
        m_RigidBody.AddForce(transform.right * slideForce * direction, ForceMode2D.Impulse);
        AnimationController.AnimatorInstance.slide = true;
        StartCoroutine("resetSlide");
    }

    IEnumerator resetSlide()
    {
        yield return new WaitForSeconds(.5f);
        controller.performing_action = false;
        AnimationController.AnimatorInstance.slide = false;

        //reset collider
        _collider.size = normalColliderScale.size;
        _collider.offset = normalColliderScale.offset;
    }

    [Serializable]
    public struct ColliderData
    {
        public Vector2 size;
        public Vector2 offset;
        public ColliderData(Vector2 _size,Vector2 _offset)
        {
            size= _size; offset= _offset;
        }
    }
}
