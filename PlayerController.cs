using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public float moveSpeed;
    public float jumpForce;
    public float gravityScale;
    public float rotateSpeed;
    public Vector3 moveDirection;          // made public for ledge grabbing

    public Transform pivot;
    public Animator anim;
    public CharacterController controller; //replacement for public Rigidbody theRB;
    public GameObject playerModel;

    [Header("Knockback")]
    public float knockbackForce;
    public float knockbackTime;
    private float knockbackCounter;


    [Header("Slope Movement")]
    private float horizontalValue;
    private float verticalValue;
    public float ySpeed;
    public float magnitude;
    private float _groundRayDistance = 1;
    private RaycastHit _slopeHit;


    [Header("Ledge Hanging")]
    public bool useGravity;

    [Header("Bounce")]
    public float bounceTime;

    [Header("Player States")]
    private bool isRunning;
    private bool isKnockbacked;
    private bool isSliding;
    public bool isHanging;
    public bool isClimbing; //still need fix



    // Start is called before the first frame update
    void Start()
    {
        //theRB = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        isRunning = false;
        isKnockbacked = false;
        isSliding = false;
        isHanging = false;
        useGravity = true;

    }

    // Update is called once per frame
    void Update()
    {

        //check if player is trying to climb
        IsPLayerTryingToClimb();

        if (useGravity)
        {
            ySpeed += Physics.gravity.y * gravityScale * Time.deltaTime;
        } else
        {
            ySpeed = 0;
        }


        Vector3 verticalDirection = transform.forward * Input.GetAxis("Vertical");
        Vector3 horizontalDirection = transform.right* Input.GetAxis("Horizontal");

        //get Input Values of horizontal and vertical
        horizontalValue = Input.GetAxis("Horizontal");
        verticalValue = Input.GetAxis("Vertical");

        //Invurnerable when hanging
        if (!isHanging)
        {
            if (knockbackCounter <= 0)
            {

                magnitude = Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"))) * moveSpeed;

                moveDirection = (verticalDirection + horizontalDirection).normalized * magnitude;


                //check if on steep slope, slide if yes
                if (controller.isGrounded && OnSteepSlope())
                {
                    SteepSlopeMovement();

                    //set sliding anim
                    isSliding = true;

                    if (horizontalValue != 0 || verticalValue != 0) // attempting to move side ways
                    {
                        SteepSlopePlusSidewaysMovement(moveDirection);

                        controller.Move(moveDirection * Time.deltaTime);
                    }


                }
                //can move using keys
                else if (controller.isGrounded && OnSteepSlope() == false)
                {
                    //set animation back
                    isSliding = false;


                    //moveDirection = new Vector3(Input.GetAxis("Horizontal") * moveSpeed, 0f, Input.GetAxis("Vertical") * moveSpeed);
                    ySpeed = -10f; // this fix the walking down "shallow" slopes by increasing value from -0.5f to -4.5f

                    if (Input.GetButtonDown("Jump") && !isClimbing)
                    {
                        ySpeed = jumpForce;
                    }
                    else if (Input.GetButtonDown("Jump") && isClimbing)
                    {
                        useGravity = false;
                        ySpeed = 0;
                        moveDirection.x = 0f;
                        moveDirection.y = moveDirection.z + 10f;
                        moveDirection.z = 0f;
                        controller.Move(moveDirection * Time.deltaTime);
                    }

                    // Set the animation based on the movement direction
                    if (moveDirection.magnitude > 0.1f)
                    {
                        isRunning = true;
                        useGravity = true;
                    }
                    else
                    {
                        isRunning = false;
                    }

                    /*if (isClimbing)
                    {
                        ySpeed = 0;
                        useGravity = false;
                        controller.Move(new Vector3(0f, moveSpeed, 0f) * Time.deltaTime);
                    }
                    else
                    {
                        isClimbing = false;
                        useGravity = true;
                        ySpeed = -10f;
                        moveDirection.y = ySpeed;
                        controller.Move(moveDirection * Time.deltaTime);
                    }*/
                }

                isKnockbacked = false;
            }
            else
            {
                //check if knocked on a steep slope
                if (OnSteepSlope())
                {
                    // knock up
                    ySpeed = jumpForce / 2;
                    isSliding = false;
                    isKnockbacked = true;
                    knockbackCounter -= Time.deltaTime;
                }
                else
                {
                    isKnockbacked = true;
                    knockbackCounter -= Time.deltaTime;
                }
            }
        }

        moveDirection.y = ySpeed;
        controller.Move(moveDirection * Time.deltaTime);
        

        //move the player in different directions based on camera look direction
        if ((moveDirection.x != 0 || moveDirection.z != 0) && isRunning)
        {
            transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
            playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
        }
        //visual bug where the player jumps first then move to a direction, player dont face towards the direction
        //fix
        else if ((moveDirection.x != 0 || moveDirection.z != 0) && !controller.isGrounded)
        {
            transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
            playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
        }

        anim.SetBool("isGrounded", controller.isGrounded);
        anim.SetBool("isRunning", isRunning);
        anim.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Vertical") + Mathf.Abs(Input.GetAxis("Horizontal"))));
        anim.SetBool("isKnockbacked", isKnockbacked);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isHanging", isHanging);
    }

    public void Knockback(Vector3 direction)
    {
        knockbackCounter = knockbackTime;

        moveDirection = direction * knockbackForce;
        moveDirection.y = knockbackForce;
    }
    

    public void BouncePlayer(float bounceForce, Vector3 direction)
    {
        knockbackCounter = bounceTime;
        //moveDirection = direction;
        ySpeed = bounceForce;
        moveDirection.y = ySpeed;
        
    }

    //for downward slope movement
    private bool OnSteepSlope()
    {
        if (!controller.isGrounded) return false;
        
        if(Physics.Raycast(transform.position, Vector3.down, out _slopeHit, (controller.height/2) + _groundRayDistance))
        {
            float _slopeAngle = Vector3.Angle(_slopeHit.normal, Vector3.up);
            if (_slopeAngle > controller.slopeLimit) return true;
        }
        return false;
    }

    private void SteepSlopeMovement()
    {
        Vector3 slopeDirection = Vector3.up -_slopeHit.normal * Vector3.Dot(Vector3.up, _slopeHit.normal);
        float slideSpeed = moveSpeed + (moveSpeed/2) + Time.deltaTime;

        moveDirection = slopeDirection * -slideSpeed;
        moveDirection.y = moveDirection.y - _slopeHit.point.y;

    }

    private Vector3 SteepSlopePlusSidewaysMovement(Vector3 slidingMovement)
    {

        //chatgpt fix
        Vector3 slopeDirection = Vector3.up - _slopeHit.normal * Vector3.Dot(Vector3.up, _slopeHit.normal);
        float slideSpeed = moveSpeed + Time.deltaTime;

        Vector3 horizontalInputDirection = transform.right * Input.GetAxis("Horizontal");
        Vector3 verticalInputDirection = transform.forward * Input.GetAxis("Vertical");
        Vector3 combinedInputDirection = (horizontalInputDirection + verticalInputDirection).normalized;

        // Condition 1: Always face away from the slope
        Quaternion targetRotation = Quaternion.LookRotation(-slopeDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        // Condition 2: Limit sliding speed when moving against the slope
        float slopeDotProduct = Vector3.Dot(slidingMovement.normalized, -slopeDirection);
        if (slopeDotProduct < 0f)
        {
            slideSpeed = Mathf.Min(slideSpeed, Mathf.Abs(slopeDotProduct * slideSpeed));
        }

        // Condition 3: Prevent sliding upwards when moving towards the slope
        float inputDotProduct = Vector3.Dot(combinedInputDirection.normalized, -slopeDirection);
        if (inputDotProduct > 0f)
        {
            combinedInputDirection = Vector3.zero;
        }

        moveDirection = slopeDirection * -slideSpeed;
        moveDirection += combinedInputDirection * (slideSpeed / 2);
        moveDirection.y = moveDirection.y - _slopeHit.point.y;

        return moveDirection;
    }

    private void IsPLayerTryingToClimb()
    {
        //if climbing

        //climbing Script
        //fire raycast in front of the character

        //for climbing script
        float avoidFloorDistance = 0.1f;
        float ladderGrabDistance = 0.4f;

        //based on the origin box, adjustment is needed for the raycast
        //to not hit the ground or any other objects that are close to
        //the ground and is not a ladder
        //(https: //www.youtube.com/watch?v=44qVzrdvm04&t=42s) 4:00

        if (Physics.Raycast(transform.position + Vector3.up * avoidFloorDistance, moveDirection, out RaycastHit hitLadder, ladderGrabDistance))
        {
            if (hitLadder.transform.TryGetComponent(out LadderClimb ladder))
            {
                //test if Raycast is working : check Log
                Debug.Log(hitLadder, transform);

                //set climbing state to true
                isClimbing = true;
                useGravity = false;
               
                //controller.Move(new Vector3(0f, moveSpeed, 0f) * Time.deltaTime);

            }
        }
        else
        {
            isClimbing = false;
            useGravity = true;
        }
    }

}




