using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtPlayer : MonoBehaviour
{
    
    public int damageToGive;

    // Start is called before the first frame update
    void Start()
    {
        damageToGive = 1 ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Vector3 hitDirection = other.transform.position - transform.position;
            hitDirection = hitDirection.normalized;

            FindObjectOfType<HealthManager>().HurtPlayer(damageToGive, hitDirection);


        }
    }
}
