using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stack")][Space(10f)]
    public Stack<GameObject> playerStack = new Stack<GameObject>();
    public Transform breadParent;
    public bool isStacking;
    public int maxStackSize;

    [Header("Bool")][Space(10f)]
    public bool playerInOven;
    public bool playerInBreadBox;
    public bool playerInCounter;

    [Header("float")][Space(10f)]
    public float moveTime;
    public float moveAnimDuration;

    [Header("Audio")][Space(10f)]
    public AudioSource audioSource;
    public AudioClip stackSound;
    public AudioClip putSound;

    private OvenController oven;
    private BreadBoxController breadBox;
    private CounterController counter;

    private void Start()
    {
        oven = EventObjectsSingleton.GetOvenController();
        breadBox = EventObjectsSingleton.GetBreadBoxController();
        counter = EventObjectsSingleton.GetCounterController();

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

            audioSource.PlayOneShot(stackSound);
            playerStack.Push(obj);
        }
    }

    private void PopPlayerStack()
    {
        if (playerStack.Count > 0 && breadBox.breadBoxQueue.Count < breadBox.maxBreadsCount)
        {
            var obj = playerStack.Pop();
            obj.transform.SetParent(breadBox.breadParent);
            PlayerPopAnim(obj);

            audioSource.PlayOneShot(putSound);
            breadBox.breadBoxQueue.Enqueue(obj);
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
        obj.transform.localRotation = Quaternion.Euler(0f, 60f, 0f);
        var goalPose = breadBox.breadPose[breadBox.breadPoseIndex++];
        breadBox.breadPoseIndex %= breadBox.maxBreadsCount;
        
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
            playerInOven = true;
        }
        else if(other.CompareTag("BreadBox"))
        {
            playerInBreadBox = true;
        }
        else if(other.CompareTag("Counter"))
        {
            playerInCounter = true;
            counter.playerInCounter = playerInCounter;
        }

    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Oven"))
        {
            playerInOven = false;
        }
        else if (other.CompareTag("BreadBox"))
        {
            playerInBreadBox = false;
        }
        else if (other.CompareTag("Counter"))
        {
            playerInCounter = false;
            counter.playerInCounter = playerInCounter;
        }
    }
}
