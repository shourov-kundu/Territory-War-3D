using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera camera;
    void LateUpdate()
    {
        transform.LookAt(camera.transform, Vector3.up);
    }
}