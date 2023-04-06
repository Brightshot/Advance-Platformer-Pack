using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    public CamShake _cam;

    public delegate void m_shake();
    public static m_shake camShake;

    public static void Shake()
    {
        camShake?.Invoke();
    }

    private void OnEnable()
    {
        DashScript.DashEvent += shake;
        camShake += shake;
    }

    private void OnDisable()
    {
        DashScript.DashEvent -= shake;
        camShake -= shake;
    }

    void shake()
    {
        StartCoroutine("initiate");
    }

    IEnumerator initiate()
    {
        _cam.Shake();
        yield return new WaitForSeconds(0.3f);
        _cam.Stop();
    }
}
