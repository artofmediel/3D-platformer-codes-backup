using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public Vector3 offset;

    public bool usedOffsetValues;

    public float rotateSpeed;

    public Transform pivot;

    public float maxViewAngle;
    public float minViewAngle;

    public bool invertY;

    // Start is called before the first frame update
    void Start()
    {
        if (!usedOffsetValues)
        {
            offset = target.position - transform.position;
        }

        pivot.transform.position = target.transform.position;
        //pivot.transform.parent = target.transform;
        pivot.transform.parent = null;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // a comment suggest to put the code here on LateUpdate function to avoid camera jittering
    void LateUpdate()
    {
        //pivot will always move with the player
        pivot.transform.position = target.transform.position;

        //get the x position of the mouse and rotate the target
        float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
        pivot.Rotate(0, horizontal, 0);
        
        
        //I disable this to prevent looking up-down feature
        /*
        //get the y position of the mouse and rotate the pivot
        float vertical = Input.GetAxis("Mouse Y") * rotateSpeed;
        //pivot.Rotate(vertical, 0, 0);
        if (invertY)
        {
            pivot.Rotate(-vertical, 0, 0);
        }
        else {
            pivot.Rotate(vertical, 0, 0);
        }

        //Limit Up-Down camera rotation
        if (pivot.rotation.eulerAngles.x > maxViewAngle && pivot.rotation.eulerAngles.x < 180f)
        {
            //pivot.rotation = Quaternion.Euler(maxViewAngle, 0, 0);
            pivot.rotation = Quaternion.Euler(maxViewAngle, target.rotation.eulerAngles.y, 0);
        }

        if (pivot.rotation.eulerAngles.x > 180f && pivot.rotation.eulerAngles.x < 360f + minViewAngle)
        {
            //pivot.rotation = Quaternion.Euler(360f + minViewAngle, 0, 0);
            pivot.rotation = Quaternion.Euler(360f + minViewAngle, target.rotation.eulerAngles.y, 0);
        }*/
        

        //move the camera based on the current rotation of the target and the original offset
        float desiredYAngle = pivot.eulerAngles.y;
        //float desiredXAngle = pivot.eulerAngles.x;

        Quaternion rotation = Quaternion.Euler(0, desiredYAngle, 0);
        transform.position = target.position - (rotation * offset);

        //transform.position = target.position - offset;
        

        // Make sure the camera is above the target
        if (transform.position.y < target.position.y)
        { 
            transform.position = new Vector3(transform.position.x, target.position.y -0.3f, target.position.z);
        }

        transform.LookAt(target);
    }
}
