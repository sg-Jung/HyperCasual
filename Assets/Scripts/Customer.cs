using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public enum QuestState
{
    TakeBread,
    PayAtCheckout,
    EatInRestaurant,
    Leave
}

public class Customer : MonoBehaviour
{
    [Header("UI")]
    public GameObject iconParent;
    public GameObject ballon;
    public GameObject payImage;
    public GameObject tableImage;
    public GameObject breadImage;
    public TMP_Text breadCountText;
    public ParticleSystem smileEmoji;

    [Header("Customer Info")]
    public QuestState currentQuestState;
    public Stack<GameObject> customerStack = new Stack<GameObject>();
    public int neededBreadCount;
    public float moveAnimDuration;
    public Transform stackObjParent;
    public Vector3 goalPose;
    public Vector3 goalObjectPose;

    [Header("Bool")]
    public bool isStacking;
    public bool isMove;
    public bool isTalking;
    public bool heWantDining;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip stackSound;
    
    private Animator anim;
    private NavMeshAgent nav;
    private BreadBoxController breadBox;
    private CounterController counter;
    private CustomerManager customerManager;

    void Awake()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        breadBox = EventObjectsSingleton.GetBreadBoxController();
        counter = EventObjectsSingleton.GetCounterController();
        customerManager = ManagerSingleton.GetCustomerManager();

        SetAllUIActive(false);
    }

    void Update()
    {
        iconParent.transform.rotation = Quaternion.identity;
        CustomerState();
        AnimationSelect();
    }

    public void SetAllUIActive(bool flag)
    {
        ballon.SetActive(flag);
        payImage.SetActive(flag);
        tableImage.SetActive(flag);
        breadImage.SetActive(flag);
        breadCountText.gameObject.SetActive(flag);
        smileEmoji.gameObject.SetActive(flag);
    }

    private void AnimationSelect()
    {
        if (anim == null) return;
        isStacking = customerStack.Count > 0 ? true : false;

        Vector3 dir = goalObjectPose - transform.position;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), 3f * Time.deltaTime);

        anim.SetBool("isMove", isMove);
        anim.SetBool("isStack", isStacking);
        anim.SetBool("isTalking", isTalking);
    }

    public void SetQuestState(QuestState newState) // 상태 변경 시 한 번 수행
    {
        currentQuestState = newState;
        Debug.Log("퀘스트 상태 변경: " + newState);

        // 상태에 따른 동작 수행
        if (newState == QuestState.TakeBread)
        {
            Debug.Log("빵을 가져가세요.");
            ballon.SetActive(true);
            breadImage.SetActive(true);
            breadCountText.gameObject.SetActive(true);

            int random = Random.Range(1, 4);
            neededBreadCount = random;

            SetBreadCountText(neededBreadCount);
            SetGoalPose(breadBox.transform.position);
        }
        else if (newState == QuestState.PayAtCheckout)
        {
            Debug.Log("계산대에서 계산하세요.");
            breadImage.SetActive(false);
            breadCountText.gameObject.SetActive(false);

            int random = Random.Range(0, 10);
            
            // 7:3 확률
            if(random >= 0 && random < 8) // 계산후 퇴실
            {
                heWantDining = false;
                payImage.SetActive(true);

                counter.onlyPayCustomerQueue.Enqueue(gameObject);
                counter.UpdateCustomerPositions(0);
            }
            else // 계산후 식당
            {
                heWantDining = true;
                tableImage.SetActive(true);

                counter.diningCustomerQueue.Enqueue(gameObject);
                counter.UpdateCustomerPositions(1);
            }

        }
        else if (newState == QuestState.EatInRestaurant)
        {
            Debug.Log("식당에서 식사하세요.");
        }
        else if (newState == QuestState.Leave)
        {
            Debug.Log("가게를 떠나세요.");
            ballon.SetActive(false);
            payImage.SetActive(false);
            smileEmoji.gameObject.SetActive(true);
            
            Vector3 exitPose = customerManager.customerEntrance.position;
            SetGoalPose(exitPose, exitPose);
        }
    }

    void CustomerState()
    {
        switch (currentQuestState)
        {
            case QuestState.TakeBread:
                if(CheckCustomerArrivedGoalPose() && isMove)
                {
                    transform.SetParent(breadBox.customerParent);
                    isMove = false;
                }

                break;
    
            case QuestState.PayAtCheckout:
                if (CheckCustomerArrivedGoalPose() && isMove)
                {
                    transform.SetParent(counter.customerParent);
                    isMove = false;
                }
                    // SetQuestState(QuestState.EatInRestaurant);
                break;

            case QuestState.EatInRestaurant:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // SetQuestState(QuestState.Leave);
                }
                break;

            case QuestState.Leave:
                if (CheckCustomerArrivedGoalPose() && isMove)
                {
                    isMove = false;
                    var obj = customerStack.Pop();

                    ManagerSingleton.GetObjectPool<ObjectPool>("PaperBag").ReturnObject(obj);
                    ManagerSingleton.GetObjectPool<ObjectPool>("Customer").ReturnObject(this.gameObject);
                }
                break;
        }
    }

    // index 0: 빵, 1: 종이 가방
    public void PushCustomerStack(GameObject obj, int index)
    {
        if(index == 0)
        {
            if (neededBreadCount > 0)
            {
                obj.transform.SetParent(stackObjParent);
                CustomerStackAnim(obj, index);
                DecreaseNeededBreadCount();
                audioSource.PlayOneShot(stackSound);
            }
        }
        else if(index == 1)
        {
            obj.transform.SetParent(stackObjParent);
            CustomerStackAnim(obj, index);
        }

        customerStack.Push(obj);
    }

    public void CustomerStackAnim(GameObject obj, int index)
    {
        if(index == 0)
        {
            obj.transform.localRotation = Quaternion.identity;
            var bread = obj?.GetComponent<Bread>();

            iTween.MoveTo(obj, iTween.Hash("x", 0, "y", bread.breadSize * stackObjParent.childCount, "z", 0, "islocal", true, "time", moveAnimDuration, "easetype", iTween.EaseType.easeOutQuint));
        }
        else if(index == 1)
        {
            obj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            iTween.MoveTo(obj, iTween.Hash("x", 0, "y", 0.5f, "z", 0, "islocal", true, "time", moveAnimDuration, "easetype", iTween.EaseType.easeOutQuint));
        }
        
    }

    public void CustomerPopAnim(GameObject obj, Vector3 position, float animDuration)
    {
        obj.transform.localRotation = Quaternion.identity;

        iTween.MoveTo(obj, iTween.Hash("x", position.x, "y", position.y, "z", position.z, "islocal", false, "time", animDuration, "easetype", iTween.EaseType.easeOutQuint));
    }

    public void DecreaseNeededBreadCount()
    {
        neededBreadCount = Mathf.Max(0, neededBreadCount - 1);
        SetBreadCountText(neededBreadCount);

        if(neededBreadCount <= 0) SetQuestState(QuestState.PayAtCheckout);
    }

    public void SetGoalPose(Vector3 goalObjectPose, Vector3 goalPose)
    {
        this.goalObjectPose = goalObjectPose;
        this.goalPose = goalPose;
        nav.SetDestination(goalPose);

        isMove = true;
    }

    public void SetGoalPose(Vector3 goalObjectPose)
    {
        this.goalObjectPose = goalObjectPose;
        nav.SetDestination(goalPose);

        isMove = true;
    }

    private void SetBreadCountText(int count) => breadCountText.text = count.ToString();

    private bool CheckCustomerArrivedGoalPose()
    {
        return Vector3.Distance(transform.position, goalPose) < 0.1f;
    }

}
