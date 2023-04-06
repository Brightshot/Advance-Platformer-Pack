using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectsScripts : MonoBehaviour
{
    public VolumeProfile PostProcessingVolume;

    public static EffectsScripts Instance;

    private Camera camera;
    public Transform Target { get; set; }

    #region VolumeParameters
    private LensDistortion distortion;
    #endregion

    public float LensValue { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        Instance = this;
    }

    void Start()
    {
        PostProcessingVolume.TryGet<LensDistortion>(out distortion);
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(Target == null)
        {
            Target = transform;
        }

        InScopeEffect();
    }

    private Vector2 GetScreenPosition(Transform target)
    {
        var pixelSpacePosition =  camera.WorldToScreenPoint(target.position);
        var normalizedPosition = new Vector2
        {
            x  = Mathf.Clamp(pixelSpacePosition.x / Screen.width,0,1),
            y = Mathf.Clamp(pixelSpacePosition.y / Screen.height,0,1)
        };
        return normalizedPosition;
    }

    private void InScopeEffect()
    {
        distortion.intensity.value = Mathf.MoveTowards(distortion.intensity.value, LensValue, 1);
        distortion.center.value = Vector2.Lerp(distortion.center.value,GetScreenPosition(Target),Time.deltaTime * 3);
    }
}