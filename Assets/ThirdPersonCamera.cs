using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    // public CharacterController controller;
    // public float speed = 6f;
    // public float turnSmoothTime = 0.1f;
    // float turnSmoothVelocity;

    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    public CameraStyle currentStyle;
    public enum CameraStyle{Basic, Combat, Topdown}
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update(){
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * vertical + orientation.right * horizontal;
        Vector3 v = player.GetComponent<Rigidbody>().velocity;
        v.y = 0f;
        if (inputDir != Vector3.zero){            
            player.forward = inputDir.normalized;
        }
        else if (v.magnitude > .01f){
            player.forward = v.normalized;
        }
    }
}
