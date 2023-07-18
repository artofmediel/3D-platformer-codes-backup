using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")]
    public PlayerController pc;     //reference to player movement script
    public Transform orientation;   //keep track of where the player is facing
    public Transform cam;
    public CharacterController controller; //Rigidbody replacement

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    public float allowedTimeOnLedge;
    private float timeOnLedge;

    public bool holding;
    private float storeRotateSpeed;
    private float playerMagnitude;
    private Vector3 directionToLedge;
    private float canGrabAnotherLedgeTimer; // fix the quick grabing issue when jumping from one ledge to another

    [Header("Ledge Jumping")]
    public KeyCode jumpKey = KeyCode.Space;
    private bool jumpingKeyDown;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;
    private float distanceToLedge;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer;

    private void Start()
    {
        controller = GetComponentInParent<CharacterController>();
        storeRotateSpeed = pc.rotateSpeed;


        exitingLedge = true;
    }

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();

        playerMagnitude = pc.magnitude;

        if (Input.GetButtonDown("Jump"))
        {
            jumpingKeyDown = true;
            pc.rotateSpeed = storeRotateSpeed;

        } else
        {
            jumpingKeyDown = false;
        }

        canGrabAnotherLedgeTimer = exitLedgeTimer;
    }

    private void SubStateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        bool anyInputKeyPressed = horizontalInput !=0 || verticalInput !=0;

        /*
        //Substate 1 - Holding on to a ledge
        if (holding)
        {
            FreezeCharacterControllerOnLedge();

            //disable player rotation
            //bug: orientation not locked and facing the ledge
            pc.rotateSpeed = 0.0f;

            timeOnLedge += Time.deltaTime;

            
            if (timeOnLedge >= minTimeOnLedge)
            {
                if (anyInputKeyPressed) ExitLedgeHold();

                if (timeOnLedge > allowedTimeOnLedge) ExitLedgeHold();

                if (Input.GetKeyDown(jumpKey)) LedgeJump();

                if (jumpingKeyDown) LedgeJump();

                if (Input.GetKeyDown(jumpKey)) LedgeJump();

                if (anyInputKeyPressed && jumpingKeyDown) LedgeJump();
            }
        }

        // Substate 2 - Existing Ledge
        else if (exitingLedge)
        {
            if (exitLedgeTime > 0)
            {
                exitLedgeTimer -= Time.deltaTime; // FLAG:: this will continue to run
                if(exitLedgeTimer <= -0.7f)
                {
                    exitLedgeTimer = -0.7f;
                }
            }
            else exitingLedge = false;
        }
        */

        // Substate 1 - Holding on to a ledge
        if (holding)
        {
            FreezeCharacterControllerOnLedge();

            // Disable player rotation
            pc.rotateSpeed = 0.0f;

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge >= minTimeOnLedge)
            {
                if (anyInputKeyPressed && Input.GetKeyDown(jumpKey))
                {
                    LedgeJump();
                }
                else if (jumpingKeyDown)
                {
                    LedgeJump();
                }
                else
                {
                    //Ended up disabling the thing instead: player need to jump to get off the ledge
                    /*if (anyInputKeyPressed)
                    {
                        ExitLedgeHold();
                    }*/

                    if (timeOnLedge > allowedTimeOnLedge)
                    {
                        ExitLedgeHold();
                    }
                }
            }
        }

        // Substate 2 - Exiting Ledge
        else if (exitingLedge)
        {
            if (exitLedgeTime > 0)
            {
                exitLedgeTimer -= Time.deltaTime;
                if (exitLedgeTimer <= -0.7f)
                {
                    exitLedgeTimer = -0.7f;
                }
            }
            else
            {
                exitingLedge = false;
            }
        }
    }


    private void LedgeDetection()
    {
        //can detect ledge if player can grab again timer condition is met
        //to prevent quick grabs
        if (canGrabAnotherLedgeTimer <= -0.1f)
        {
            if (currLedge != lastLedge || exitingLedge)
            {
                //causes issue where camera needs to face the ledge too in order to do the hold
                //bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);
                //chatgpt modified
                bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, pc.playerModel.transform.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);
                //uses transform.position instead

                if (!ledgeDetected) return; // if didnt detect ledge , skip the whole function

                float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

                if (ledgeHit.transform == lastLedge) return;

                if (distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
            }
        }
    }

    /*
    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(delayedJumpForce), 0.05f);

    }*/

    //chatgpt fix
    private void LedgeJump()
    {
        pc.rotateSpeed = storeRotateSpeed;

        ExitLedgeHold();

        // Reset the moveDirection of the player first
        pc.moveDirection = Vector3.zero;

        Invoke(nameof(delayedJumpForce), 0.05f);
    }

    private void delayedJumpForce()
    {
        //reset the moveDirection of player first
        pc.moveDirection = Vector3.zero;

        //set force that shoot the player upwards and forwards
        //Vector3 forceToAdd = pc.playerModel.transform.forward * ledgeJumpForwardForce + orientation.up * pc.jumpForce;

        // Set force to shoot the player upwards and forwards
        //Vector3 forceToAdd = orientation.forward * ledgeJumpForwardForce;

        /*float time = 0.0f;
        float duration = 1.0f;

        while (time < duration)
        {
            pc.moveDirection.y += pc.jumpForce;
            pc.moveDirection = pc.moveDirection.normalized;
            pc.controller.Move(pc.moveDirection * Time.deltaTime);

            time += Time.deltaTime;
        }

        pc.moveDirection.x = (currLedge.position.x - pc.transform.position.x);
        pc.moveDirection.z = (currLedge.position.z - pc.transform.position.z);
        pc.controller.Move(pc.moveDirection * Time.deltaTime);*/

        //yoink jump logic on PlayerController
        // jump
        pc.ySpeed = pc.jumpForce;

        pc.moveDirection.y = pc.ySpeed;

        pc.controller.Move(pc.moveDirection * Time.deltaTime);

        //fix jolt forward after jumping up
        StartCoroutine(JoltForward());
    }

    private IEnumerator JoltForward() 
    {

        yield return new WaitForSeconds(0.4f);

        pc.moveDirection = (pc.transform.position - currLedge.transform.position) * 10f;
        pc.moveDirection.y = currLedge.position.y - pc.transform.position.y - 0.5f;
        pc.moveDirection = pc.moveDirection.normalized;

        //pc.moveDirection = new Vector3(currLedge.position.x - pc.transform.position.x, currLedge.position.y - pc.transform.position.y - 0.5f, currLedge.position.z - pc.transform.position.z);


        pc.controller.Move(pc.moveDirection * Time.deltaTime);
    }

    private void EnterLedgeHold()
    {
        holding = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        //deactivate gravity thingy of controller
        pc.useGravity = false;
        pc.moveDirection = Vector3.zero;

        exitingLedge = false;
    }

    //chatgpt fix 
    private void FreezeCharacterControllerOnLedge()
    {
        //set isHanging anim on PlayerController to true
        pc.isHanging = true;
        pc.useGravity = false;

        //adjust y value to snap top part only of pc
        directionToLedge = new Vector3(currLedge.position.x - pc.transform.position.x, currLedge.position.y - pc.transform.position.y - 0.5f , currLedge.position.z - pc.transform.position.z);
        distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        
        // Move player towards ledge
        if (distanceToLedge > 1f)
        {
            //working snap to ledge
            pc.moveDirection = directionToLedge;
            //fix y axis snap
            pc.controller.Move(pc.moveDirection * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToLedge.x, 0f, directionToLedge.z));
            pc.transform.rotation = Quaternion.Slerp(pc.playerModel.transform.rotation, targetRotation, storeRotateSpeed * Time.deltaTime);

        }

        // Hold onto ledge
        else
        {
            if (pc.moveDirection != Vector3.zero) pc.moveDirection = Vector3.zero;
            pc.rotateSpeed = 0f;
        }

        // Exiting if something goes wrong
        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }

    private void ExitLedgeHold()
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timeOnLedge = 0f;
        distanceToLedge = 0f;
        directionToLedge = Vector3.zero;
        pc.rotateSpeed = storeRotateSpeed;

        //pc.restricted = false;
        //pc.freeze = false;

        //set isHanging anim on PlayerController to false
        pc.isHanging = false;
        pc.useGravity = true;

        //StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 0.7f); //call reset last ledge function after 1 sec
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }

    
}
