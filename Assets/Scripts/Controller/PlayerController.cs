using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Money")][Space(10f)]
    public int curMoney = 0;
    public TMP_Text moneyText;

    [Header("AnimPose")][Space(10f)]
    public Transform animMidPose;
    public Transform animEndPose;

    public Stack<GameObject> playerStack = new Stack<GameObject>();
    [Header("Stack")][Space(10f)]
    public Transform breadParent;
    public bool isStacking;
    public int maxStackSize;
    public GameObject maxText;

    [Header("Bool")][Space(10f)]
    public bool playerInOven;
    public bool playerInBreadBox;
    public bool playerInCounter;
    public bool playerInMoneyArea;
    public bool playerInDining;

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
    private DiningController dining;

    private void Start()
    {
        oven = EventObjectsSingleton.GetOvenController();
        breadBox = EventObjectsSingleton.GetBreadBoxController();
        counter = EventObjectsSingleton.GetCounterController();
        dining = EventObjectsSingleton.GetDiningController();

        StartCoroutine(MoveOvenBreadToPlayer());
        StartCoroutine(MovePlayerToBreadBox());
        SetMoneyText(curMoney);
    }

    private void Update() 
    {
        isStacking = playerStack.Count > 0 ? true : false;
        
        maxText.transform.rotation = Quaternion.identity;
        maxText.SetActive(playerStack.Count >= maxStackSize);
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

    public void SetMoney(int money)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", curMoney, "to", curMoney + money, "onUpdate", "SetMoneyText", "delay", 0, "time", 0.5));
        
        curMoney += money;
    }

    private void SetMoneyText(int money)
    {
        moneyText.text = money.ToString();
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

            breadBox.isPlayerPutBread = playerInBreadBox && playerStack.Count > 0;
            if (breadBox.isPlayerPutBread) PopPlayerStack();
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
        else if(other.CompareTag("Dining"))
        {
            playerInDining = true;
            dining.playerInDining = playerInDining;
            dining.inDiningPlayer = this;
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
        else if (other.CompareTag("Dining"))
        {
            playerInDining = false;
            dining.playerInDining = playerInDining;
            dining.inDiningPlayer = null;
        }
    }
}
