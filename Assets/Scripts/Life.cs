using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField]private float maxHealth = 100;
    [SerializeField] private float currentHealth=100;

    public delegate void PlayerDeath();
    public PlayerDeath playerDeath;

    Ragdoll doll;

    private void Start()
    {
        doll = Ragdoll.ragdoll_instance;
    }

    private void Update()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(currentHealth <= 0)
        {
            playerDeath?.Invoke();
            doll.OnRagDoll();

            DestroyAllScripts();
        }
    }

    void DestroyAllScripts()
    {
        var animator = GetComponentInChildren<Animator>();

        animator.enabled = false;

        List<MonoBehaviour> scripts = GetComponents<MonoBehaviour>().ToList();

        var childrenScripts = GetComponentsInChildren<MonoBehaviour>();

        scripts.AddRange(childrenScripts);

        foreach(var script in scripts)
        {
            if (script != this)
            {
                Destroy(script);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Danger"))
        {
            currentHealth = 0;
        }
    }
}
