using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlatformerController))]
[Serializable]
public class PlatformerControllerEditor : Editor
{
    private PlatformerController ControllerTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ControllerTarget = target as PlatformerController;

        Undo.RecordObject(ControllerTarget, this.name);
        serializedObject.Update();

        //JUMP
        
        bool isNormal = (ControllerTarget.GetComponent<AdvanceJump>()==null);
        ControllerTarget.controlsJump = isNormal;

        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(isNormal)))
        {
            if(group.visible == true)
            {
                NormalJump();
            }
        }

        MechanicsAddition();

        serializedObject.ApplyModifiedProperties();
    }

    private void MechanicsAddition()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("EXTRA MECHANICS", GUIStyle.none);
        int index = 0;
        string[] extensions = { "addExtension..", "AdvanceJump", "Dash", "GrappleHook", "Slide","Wall Climb" };
        index = EditorGUILayout.Popup("AddExtension", index, extensions);
        switch (index)
        {
            case 1:
                ControllerTarget.AddComponent<AdvanceJump>();
                break;
            case 2:
                ControllerTarget.AddComponent<DashScript>();
                break;
            case 3:
                ControllerTarget.AddComponent<GrappleHook>();
                break;
            case 4:
                ControllerTarget.AddComponent<SlideScript>();
                break;
            case 5:
                ControllerTarget.AddComponent<WallClimb>();
                break;
            default:

                break;
        }
    }

    private void NormalJump()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("JUMP");
        ControllerTarget.jumpForce = EditorGUILayout.FloatField("JumpForce", ControllerTarget.jumpForce);
        ControllerTarget.bufferTime = EditorGUILayout.Slider("BufferTime", ControllerTarget.bufferTime, 0, 1);
    }
}
