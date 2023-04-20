using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdvanceJump))]
[SerializeField]
public class AdvanceJumpEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        var controller = target as AdvanceJump;

        Undo.RecordObject(controller, this.name);

        controller.a_bufferTime = EditorGUILayout.Slider("Buffer Time", controller.a_bufferTime, 0, 1);
        EditorGUILayout.Space(10);

        switch (controller.jumpType)
        {
            case Jump_Types.NormalJump:

                controller.force[0] =  EditorGUILayout.FloatField("primary Force", controller.force[0]);

                break;
            case Jump_Types.DoubleJump:

                controller.force[0] = EditorGUILayout.FloatField("primary Force", controller.force[0]);
                controller.force[1] = EditorGUILayout.FloatField("secondary Force", controller.force[1]);

                break;
            case Jump_Types.ExtendedJump:
                controller.force[2] = EditorGUILayout.FloatField("JumpTime", controller.force[2]);
                break;
            default:
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
