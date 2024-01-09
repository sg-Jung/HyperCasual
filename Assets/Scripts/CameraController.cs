using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float y;
    public float z;

    // Update is called once per frame
    void Update()
    {
        Vector3 newPose = new Vector3(target.position.x, target.position.y + y, target.position.z + z);
        transform.position = newPose;
    }
}
