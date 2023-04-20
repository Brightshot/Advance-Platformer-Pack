using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class EffectsScripts : MonoBehaviour
{
    public VolumeProfile PostProcessingVolume;

    [Header("Vignette effect")]
    public AnimationCurve panelAnimation;
    public Image panel;
    public float maxIntensity;

    public static EffectsScripts Instance;

    private Camera cam;
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
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(Target == null)
        {
            Target = transform;
        }

        InScopeEffect();
        VignetteEffect();
    }

    private float _time = 0;
    public bool isAnimating { get; set; }
    void VignetteEffect()
    {
        if(isAnimating)
        {
            _time += Time.deltaTime;
        }
        else
        {
            _time = 0;
            var a = new Color { a = 0, r = panel.color.r, g = panel.color.g, b = panel.color.b };
            panel.color = a;
        }

        Color alpha = panel.color;
        alpha.a = panelAnimation.Evaluate(_time) * maxIntensity;
        panel.color = alpha;
    }

    private Vector2 GetScreenPosition(Transform target)
    {
        var pixelSpacePosition =  cam.WorldToScreenPoint(target.position);
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