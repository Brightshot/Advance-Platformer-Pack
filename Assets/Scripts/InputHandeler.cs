using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class InputHandeler : MonoBehaviour
{
    public static Vector2 InputAxis { get; private set; }

    public Joystick MovementJoystick;
    public Joystick AimingJoystick;

    [Space]
    public float FireDeadZone=0.5f;

    public enum type { Joystcik, Mouse_And_Keyboard };
    public type ControlType;

    public static InputHandeler instance;

    public static Vector2 AimDirection { get; private set; }
    public static bool Firing;

    #region events

    public delegate void jump();
    public jump JumpAction;

    #endregion

    private void Awake()
    {
        instance= this;
    }

    private void Update()
    {
        if (ControlType == type.Mouse_And_Keyboard)
        {
            KeyBoard();
        }else if(ControlType == type.Joystcik)
        {
            Joystick();
        }
    }

    private void Joystick()
    {
        InputAxis = new Vector2
        {
            x = MovementJoystick.Horizontal,
            y = MovementJoystick.Vertical
        };

        AimDirection = new Vector2
        {
            x = AimingJoystick.Horizontal,
            y = AimingJoystick.Vertical
        };

        Firing = ( (Mathf.Abs(AimDirection.magnitude)) >= FireDeadZone )  ? true : false;

    }

    private void KeyBoard()
    {

        #region Vectors
        InputAxis = new Vector2
        {
            x = Input.GetAxis("Horizontal"),
            y = Input.GetAxis("Vertical")
        };

        AimDirection = -(transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition));
        #endregion

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpAction?.Invoke();
        }

        if (Input.GetMouseButton(0))
        {
            Firing = true;
        }
        else
        {
            Firing = false;
        }
    }

    public virtual void FireReset()
    {
        //Do something
    }
}
