using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    public CamShake _cam;
    public CinemachineVirtualCamera virtualCamera;

    public delegate void m_shake();
    public static m_shake camShake;

    public static void Shake()
    {
        camShake?.Invoke();
    }

    private void OnEnable()
    {
        DashScript.dash_Event += shake;
        camShake += shake;
    }

    private void OnDisable()
    {
        DashScript.dash_Event -= shake;
        camShake -= shake;
    }

    void shake()
    {
        StartCoroutine("initiate");
        StartCoroutine("initiateCine");
    }

    IEnumerator initiate()
    {
        if (_cam != null)
        {
            _cam.Shake();
            yield return new WaitForSeconds(0.3f);
            _cam.Stop();
        }
    }

    IEnumerator initiateCine()
    {
        if (virtualCamera != null)
        {
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 5.0f;
            yield return new WaitForSeconds(0.3f);
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.0f;
        }
    }
}
