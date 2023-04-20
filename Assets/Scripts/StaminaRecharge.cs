using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaRecharge : MonoBehaviour
{
    public float charge=10;
    public GameObject particle;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(collision.transform.parent.GetComponent<StaminaScript>() != null) 
            {
                collision.transform.parent.GetComponent<StaminaScript>().changeValue(charge);
                Instantiate(particle,transform.position,Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
