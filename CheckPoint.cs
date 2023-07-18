using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public HealthManager theHealthManager;

    public Renderer checkpointRenderer;

    public Material checkpointOff;
    public Material checkpointOn;


    // Start is called before the first frame update
    void Start()
    {
        theHealthManager = FindObjectOfType<HealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckPointOn()
    {
        CheckPoint[] checkpointPoints = FindObjectsOfType<CheckPoint>();
        foreach(CheckPoint cp in checkpointPoints)
        {
            cp.CheckPointOff();
        }

        checkpointRenderer.material = checkpointOn;
    }

    public void CheckPointOff()
    {
        checkpointRenderer.material = checkpointOff;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player")) { 
            theHealthManager.SetSpawnPoint(transform.position);
            CheckPointOn();
        }
    }
}
