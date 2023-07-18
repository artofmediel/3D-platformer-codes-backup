using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePlayer : MonoBehaviour
{
    public float bounceForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Vector3 bounceDirection = other.transform.position - transform.position;
            bounceDirection = bounceDirection.normalized;

            FindObjectOfType<HealthManager>().BouncePlayer(bounceForce, bounceDirection);


        }
    }
}
