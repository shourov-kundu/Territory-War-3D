using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed;
    public Transform orientation;
    public float playerHeight;
    public LayerMask whatIsGround;
    public float groundDrag;
    float horizontal;
    float vertical;
    Vector3 moveDirection;
    Rigidbody rb;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
    }
    private void MyInput(){        
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && IsGrounded()){
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void MovePlayer(){
        moveDirection = orientation.forward * vertical + orientation.right * horizontal;
        Vector3 forceVector = moveDirection.normalized * moveSpeed;
        if (!IsGrounded()){
            forceVector *= airMultiplier;
        }
        rb.AddForce(forceVector, ForceMode.VelocityChange);
    }
    void Update(){
        MyInput();
        SpeedControl();
        if (IsGrounded())
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }
    void FixedUpdate(){
        MovePlayer();   
    }
    private void SpeedControl(){
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed) {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
    private void Jump(){
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump(){
        readyToJump = true;
    }
    public bool IsGrounded(){
        return Physics.Raycast(transform.position,Vector3.down, playerHeight*.5f+.2f, whatIsGround);
    }
}
