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
    public Vector3 goalDirPose;

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
    private DiningController dining;

    void Awake()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        breadBox = EventObjectsSingleton.GetBreadBoxController();
        counter = EventObjectsSingleton.GetCounterController();
        dining = EventObjectsSingleton.GetDiningController();
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

        Vector3 desiredVelocity = nav.velocity;
        desiredVelocity.y = 0f; // y 축 회전을 고려하지 않음

        if(nav.velocity.magnitude > 0.1f)
        {
            if (desiredVelocity != Vector3.zero)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(desiredVelocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * 5f);
            }
        }

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
            dining.diningWaitQueue.Enqueue(gameObject);
            dining.UpdateCustomerPositions();
        }
        else if (newState == QuestState.Leave)
        {
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
                    CustomerArrivedPlace(breadBox.customerParent);
                }

                break;
    
            case QuestState.PayAtCheckout:
                if (CheckCustomerArrivedGoalPose() && isMove)
                {
                    CustomerArrivedPlace(counter.customerParent);
                }
                break;

            case QuestState.EatInRestaurant:
                if (CheckCustomerArrivedGoalPose() && isMove)
                {
                    CustomerArrivedPlace(dining.customerParent);
                }
                break;

            case QuestState.Leave:
                if (CheckCustomerArrivedGoalPose() && isMove)
                {
                    isMove = false;
                    
                    PopAllCustomerStack();
                    ManagerSingleton.GetObjectPool<ObjectPool>("Customer").ReturnObject(this.gameObject);
                }
                break;
        }
    }

    public void CustomerArrivedPlace(Transform parent)
    {
        transform.SetParent(parent);
        transform.LookAt(goalDirPose);
        isMove = false;
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

    public void PopAllCustomerStack()
    {
        if(customerStack.Count <= 0) return;

        while(customerStack.Count > 0)
        {
            var obj = customerStack.Pop();

            if(obj.name.Equals("PaperBag"))
            {
                ManagerSingleton.GetObjectPool<ObjectPool>("PaperBag").ReturnObject(obj);
            }
        }
    }

    public void DecreaseNeededBreadCount()
    {
        neededBreadCount = Mathf.Max(0, neededBreadCount - 1);
        SetBreadCountText(neededBreadCount);

        if(neededBreadCount <= 0) SetQuestState(QuestState.PayAtCheckout);
    }

    public void SetGoalPose(Vector3 goalDirPose, Vector3 goalPose)
    {
        this.goalDirPose = goalDirPose;
        this.goalPose = goalPose;
        nav.SetDestination(goalPose);

        isMove = true;
    }

    public void SetGoalPose(Vector3 goalDirPose)
    {
        this.goalDirPose = goalDirPose;
        nav.SetDestination(goalPose);

        isMove = true;
    }

    private void SetBreadCountText(int count) => breadCountText.text = count.ToString();

    private bool CheckCustomerArrivedGoalPose()
    {
        return Vector3.Distance(transform.position, goalPose) < 0.1f;
    }

}
