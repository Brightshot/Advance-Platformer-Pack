using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform Sparks;
    public Transform laserEnd;
    [SerializeField] private int resolution = 10;
    [SerializeField] private float amplitude = 2;
    [SerializeField] private float frequency = 1;
    [SerializeField] private float speed =1;


    // Update is called once per frame
    void Update()
    {
        DrawWaves();
    }

    void DrawWaves()
    {
        lineRenderer.positionCount = resolution;

        var localPosition = -(transform.position - laserEnd.position);

        for (int i = 0; i < resolution; i++)
        {
            float delta = (float)i / (resolution - 1);


            var offset = Vector2.Perpendicular(localPosition).normalized;
            var targetPos = Vector2.Lerp(Vector2.zero, localPosition, delta);

            float dx = Mathf.Lerp(0, localPosition.magnitude, delta);
            float dy = Mathf.Sin((delta * frequency) + (Time.timeSinceLevelLoad*speed)) * amplitude;

            var currentPos = (new Vector2(targetPos.x, targetPos.y)) + (offset * dy);

            lineRenderer.SetPosition(i, currentPos + (Vector2)transform.position);
        }

        var ray = Physics2D.Raycast(transform.position, transform.right, 20);

        if(ray.collider != null)
        {
            if (ray.collider.transform.root.CompareTag("Player"))
            {
                StopCoroutine("Deactivate");
                Sparks.position = new Vector2(ray.collider.transform.position.x, Sparks.position.y);
                Sparks.gameObject.SetActive(true);

                if (!hit)
                {
                    EffectsScripts.Instance.panel.color = Color.red;
                    ShakeController.Shake();
                    hit= true;
                }

                EffectsScripts.Instance.isAnimating = true;
            }
            else
            {
                hit = false;
                StartCoroutine("Deactivate");
            }
        }
    }

    bool hit;

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1f);
        Sparks.gameObject.SetActive(false);
    }
}
