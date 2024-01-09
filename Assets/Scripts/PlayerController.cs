using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Stack<GameObject> playerStack = new Stack<GameObject>();
    public Transform breadParent;
    public bool isStacking;
    public int maxStackSize;

    public bool playerInOven;
    public bool playerInBreadBox;
    
    public float moveTime;
    public float moveAnimDuration;

    private OvenController oven;
    private BreadBoxController bBoxController;

    private void Start()
    {
        StartCoroutine(MoveOvenBreadToPlayer());
        StartCoroutine(MovePlayerToBreadBox());
    }

    private void Update() 
    {
        isStacking = playerStack.Count > 0 ? true : false;
    }

    private void PushPlayerStack()
    {
        if (playerStack.Count < maxStackSize)
        {
            var obj = (oven.ovenBreads.Count > 0) ? oven.ovenBreads.Dequeue() : null;
            if (obj == null) return;

            obj.transform.SetParent(breadParent);
            PlayerStackAnim(obj); // Player 쪽으로 이동하는 애니메이션

            playerStack.Push(obj);
        }
    }

    private void PopPlayerStack()
    {
        if (playerStack.Count > 0 && bBoxController.breadBoxQueue.Count < bBoxController.maxBreadsCount)
        {
            var obj = playerStack.Pop();
            obj.transform.SetParent(bBoxController.breadParent);
            PlayerPopAnim(obj);

            bBoxController.breadBoxQueue.Enqueue(obj);
        }
    }

    public void PlayerStackAnim(GameObject obj) // obj는 애니메이션을 적용시킬 Bread
    {
        obj.transform.localRotation = Quaternion.identity;
        var bread = obj?.GetComponent<Bread>();
        bread?.SetBreadColliderRigidBodyEnable(false);

        iTween.MoveTo(obj, iTween.Hash("x", 0, "y", bread.breadSize * breadParent.childCount, "z", 0, "islocal", true, "time", moveAnimDuration, "easetype", iTween.EaseType.easeOutQuint));
    }

    public void PlayerPopAnim(GameObject obj) // obj는 애니메이션을 적용시킬 Bread
    {
        obj.transform.localRotation = Quaternion.Euler(new Vector3(0f, 60f, 0f));
        var goalPose = bBoxController.breadPose[bBoxController.breadBoxQueue.Count];

        iTween.MoveTo(obj, iTween.Hash("x", goalPose.x, "y", 0, "z", goalPose.z, "islocal", true, "time", moveAnimDuration, "easetype", iTween.EaseType.easeOutQuint));
    }

    private IEnumerator MoveOvenBreadToPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveTime);

            if (playerInOven)
            {
                PushPlayerStack();
            }
        }
    }

    private IEnumerator MovePlayerToBreadBox()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveTime);

            if (playerInBreadBox)
            {
                PopPlayerStack();
            }
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Oven"))
        {
            oven = other.GetComponent<OvenController>();
            playerInOven = true;
        }
        else if(other.CompareTag("BreadBox"))
        {
            bBoxController = other.GetComponent<BreadBoxController>();
            playerInBreadBox = true;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Oven"))
        {
            playerInOven = false;
            oven = null;
        }
        else if (other.CompareTag("BreadBox"))
        {
            playerInBreadBox = false;
            bBoxController = null;
        }
    }
}
