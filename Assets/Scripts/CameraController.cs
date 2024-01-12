using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform player;
    public float y;
    public float z;

    public bool cameraLock;

    private void Start() 
    {
        target = player;
    }

    void Update()
    {
        if(!cameraLock)
        {
            Vector3 newPose = new Vector3(player.position.x, player.position.y + y, player.position.z + z);
            transform.position = newPose;
        }
        else
        {
            Vector3 newPose = new Vector3(target.position.x, target.position.y + y, target.position.z + z);
            transform.position = Vector3.Lerp(transform.position, newPose, Time.deltaTime * 2f);
        }
    }

    public void SetCameraLockAndNewTarget(bool flag, Transform newTarget = null)
    {
        cameraLock = flag;
        target = newTarget ?? player;
    }
}
