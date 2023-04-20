using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public Transform parent;

    public Rigidbody2D[] bodyParts;
    public static Ragdoll ragdoll_instance;

    private void Awake()
    {
        ragdoll_instance= this;
    }

    [ContextMenu("Off RagDoll")]
    private void Off()
    {
        OffRagDoll();
    }

    public  async void OnRagDoll()
    {
        Destroy(parent.GetComponent<Collider2D>());
        await Task.Yield();
        foreach (var part in bodyParts)
        {
            part.simulated = true;
        }
    }

    public async void OffRagDoll()
    {
        await Task.Yield();
        foreach (var part in bodyParts)
        {
            part.simulated = false;
        }
    }
}
