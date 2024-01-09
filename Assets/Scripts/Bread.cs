using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bread : MonoBehaviour
{
    public float breadSize;

    public void SetBreadColliderRigidBodyEnable(bool flag)
    {
        gameObject.GetComponent<BoxCollider>().enabled = flag;
        gameObject.transform.localEulerAngles = Vector3.zero;
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = flag;
        rb.constraints = flag ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll; // flag가 true면 Freeze 전부 해제, false면 Freeze 전부 체크
    }
}
