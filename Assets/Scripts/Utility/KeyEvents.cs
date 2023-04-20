using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyEvents
{

    #region KeyCallEvents

    //Stores key Data
    public class isPressed
    {
        public string id;
        public bool pressed;

        public isPressed(string id, bool pressed)
        {
            this.pressed = pressed;
            this.id = id;
        }
    }

    public static bool pressed(string key)    //return true if "key" is present in list of called keys
    {
        if (KeysCalled.ContainsKey(key))
        {
            if (KeysCalled[key].pressed == true)
            {
                return true;
            }
        }

        return false;
    }

    public static void CallKey(MonoBehaviour caller,string key,float delay)
    {
        caller.StartCoroutine(key,delay);
    }

    public static IEnumerator KeyPressed(string key, float delay)
    {
        return PressedThisFrame(key, delay);
    }

    public static Dictionary<string, isPressed> KeysCalled = new Dictionary<string, isPressed>();    //stores all key events

    private static IEnumerator PressedThisFrame(string key, float delay)
    {
        if (!KeysCalled.ContainsKey(key))
        {
            var thisKeyPressed = new isPressed(key, true);   //Create a new key event with the called id "key" and set pressed to true

            KeysCalled.Add(key, thisKeyPressed); //add the key event to stored key events "KeysCalled"

            /*Debug.Log("performing");*/

            yield return new WaitForSeconds(delay);

            /*Debug.Log("Done");*/

            if (KeysCalled.ContainsKey(key))
            {
                KeysCalled[key] = new isPressed(key, false);    //set the called key false after delay
                KeysCalled.Remove(key);     //Remove called key from stored key events
            }
        }
    }
    #endregion

}