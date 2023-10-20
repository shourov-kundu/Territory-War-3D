using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera Camera;

    void Update()
    {
        transform.LookAt(Camera.transform, Vector3.up);
    }
}