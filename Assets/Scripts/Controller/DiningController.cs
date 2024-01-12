using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiningController : MonoBehaviour
{
    [Header("Transform")]
    public Transform diningWaitPose;
    public Transform diningPose;
    public Transform tablePose;
    public Transform diningBreadPose;
    public Transform customerParent;
    public Transform diningStackParent;

    [Header("Object")]
    public GameObject ActiveDining;
    public GameObject DeActiveDining;
    public GameObject trash;
    public ParticleSystem trashParticle;

    [Header("Money")]
    public MoneyArea moneyArea;

    [Header("Float")]
    public float distanceBetweenCustomers;
    public float waitingTime;
    public float customerPopAnimTime;
    public float pushDiningAnimDuration;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clearSound;
    public AudioClip payMentSound;

    public bool isActive;
    public bool isTrashOnTheTable;
    public bool playerInDining;

    public Queue<GameObject> diningWaitQueue = new Queue<GameObject>();
    public Stack<GameObject> diningStack = new Stack<GameObject>();
    public PlayerController inDiningPlayer;

    private LockMoneyAreaManager lockMoneyAreaManager;
    private ObjectPool breadPool;

    private void Start() 
    {
        lockMoneyAreaManager = ManagerSingleton.GetLockMoneyAreaManager();
        breadPool = ManagerSingleton.GetObjectPool<ObjectPool>("Bread");

        StartCoroutine(CustomerEatingInDining());
        StartCoroutine(PlayerCleanTable());
        StartCoroutine(WaitingLock());

        if(!isActive)
        {
            ActiveDining.SetActive(false);
            DeActiveDining.SetActive(true);
        }
    }

    private IEnumerator CustomerEatingInDining()
    {
        while (true)
        {
            yield return new WaitUntil(() => diningWaitQueue.Count > 0 && !isTrashOnTheTable);

            if (diningWaitQueue.Count > 0)
            {
                // 식당에서 식사를 원하는 고객
                Customer customer = diningWaitQueue.Peek().GetComponent<Customer>();
                
                // 맨 앞자리에 있지 않다면 continue
                if(Vector3.Distance(customer.transform.position, diningWaitPose.position) > 0.1f) continue;

                // 대기 큐에서 빼주고 테이블 앞으로 이동 후 대기 큐에 있는 고객들의 자리를 한 칸씩 앞으로 이동
                diningWaitQueue.Dequeue();
                customer.SetGoalPose(diningPose.localPosition, diningPose.position);
                UpdateCustomerPositions();

                // 테이블 앞에 올때까지 대기
                yield return new WaitUntil(() => Vector3.Distance(customer.transform.position, diningPose.position) <= 0.1f);

                // 선택한 고객의 스택에 있는 모든 빵들을 테이블 위에 올려놓는 코드
                while (customer.customerStack.Count > 0)
                {
                    var bread = customer.customerStack.Pop();
                    // 고객의 스택에서 빼낸 빵을 diningStack에 Push
                    PushDiningStack(bread);
                    yield return new WaitForSeconds(customerPopAnimTime + 0.05f);
                }

                // 빵을 테이블에 전부 올려놓으면 먹는 애니메이션 시작
                customer.isTalking = true;
                yield return new WaitForSeconds(waitingTime); // 먹는 애니메이션 동작시간
                customer.isTalking = false;

                // 빵을 다 먹은 후 동작

                // 스택에 있는 것들을 pool에 전부 반환하고 쓰레기를 활성화
                PopAllDiningStack();
                trash.SetActive(true);

                isTrashOnTheTable = true;
                audioSource.PlayOneShot(payMentSound);
                moneyArea.PayForPlayer();

                customer.SetQuestState(QuestState.Leave);

            }
        }
    }

    private IEnumerator PlayerCleanTable()
    {
        while(true)
        {
            // 플레이어가 테이블 주위에 있고, 쓰레기가 테이블 위에 올려 있다면 넘어감
            yield return new WaitUntil(() => inDiningPlayer != null && Vector3.Distance(inDiningPlayer.transform.position, tablePose.position) <= 1.7f && isTrashOnTheTable);

            yield return new WaitForSeconds(0.15f); // 약간 텀을 두고 쓰레기 정리
            trash.SetActive(false);
            trashParticle.Play();
            audioSource.PlayOneShot(clearSound);

            isTrashOnTheTable = false;
        }
    }

    public void PushDiningStack(GameObject obj)
    {
        obj.transform.SetParent(diningStackParent);
        DiningStackAnim(obj);
        diningStack.Push(obj);
    }

    public void DiningStackAnim(GameObject obj)
    {
        var bread = obj?.GetComponent<Bread>();
        obj.transform.localRotation = Quaternion.Euler(0f, 60f, 0f);
        Vector3 position = diningBreadPose.localPosition;

        iTween.MoveTo(obj, iTween.Hash("x", position.x, "y", position.y + (bread.breadSize * diningStackParent.childCount), "z", position.z, "islocal", true, "time", pushDiningAnimDuration, "easetype", iTween.EaseType.easeOutQuint));
    }


    private void PopAllDiningStack()
    {
        if(diningStack.Count <= 0) return;

        while(diningStack.Count > 0)
        {
            breadPool.ReturnObject(diningStack.Pop());
        }
    }

    public void UpdateCustomerPositions() // 줄 서 있는 고객의 위치를 업데이트
    {
        int customerIndex = 0;

        foreach (var c in diningWaitQueue)
        {
            Customer customer = c.GetComponent<Customer>();
            Vector3 queueStartPoint = diningWaitPose.position;
            float zPosition = queueStartPoint.z + customerIndex * distanceBetweenCustomers;

            Vector3 newPosition = new Vector3(queueStartPoint.x, queueStartPoint.y, zPosition);
            customer.SetGoalPose(transform.position, newPosition);

            customerIndex++;
        }
    }

    private IEnumerator WaitingLock()
    {
        // currentLockSequence가 First인 동안 대기, First가 아니게 되면 넘어감
        yield return new WaitWhile(() => lockMoneyAreaManager.currentLockSequence == LockSequence.First);
        isActive = true;
        ActiveDining.SetActive(true);
        DeActiveDining.SetActive(false);

        // lockMoneyArea.transform.localPosition = nextLockMoneyArea;
        // lockMoneyArea.SetNeedMoney(nextLockMoney);
        // lockMoneyArea.SetActiveAndLock(true);

        // yield return new WaitUntil(() => !lockMoneyArea.isLock);
    }
}
