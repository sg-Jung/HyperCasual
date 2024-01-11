using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterController : MonoBehaviour
{
    [Header("Transform")][Space(10f)]
    public Transform[] counterPose;
    public Transform customerParent;
    public Transform paperBagPose;
    public Transform paperBagEntrancePose;

    [Header("Money")][Space(10f)]
    public MoneyArea moneyArea;

    public Queue<GameObject> onlyPayCustomerQueue = new Queue<GameObject>();
    public Queue<GameObject> diningCustomerQueue = new Queue<GameObject>();

    [Header("Float")][Space(10f)]
    public float distanceBetweenCustomers;
    public float payingTime;
    public float customerPopAnimTime;
    public bool playerInCounter;

    [Header("Audio")][Space(10f)]
    public AudioSource audioSource;
    public AudioClip cashSound;

    private ObjectPool paperBagPool;
    private ObjectPool breadPool;
    private ObjectPool moneyPool;

    private DiningController dining;

    private void Start() 
    {
        paperBagPool = ManagerSingleton.GetObjectPool<ObjectPool>("PaperBag");
        breadPool = ManagerSingleton.GetObjectPool<ObjectPool>("Bread");
        moneyPool = ManagerSingleton.GetObjectPool<ObjectPool>("Money");

        dining = EventObjectsSingleton.GetDiningController();

        StartCoroutine(PlayerPayingForCustomer());
    }

    private IEnumerator PlayerPayingForCustomer()
    {
        while(true)
        {
            yield return new WaitUntil(() => playerInCounter);
            if(onlyPayCustomerQueue.Count <= 0 && diningCustomerQueue.Count <= 0) continue;
            
            yield return new WaitForSeconds(payingTime);
//-----------------------------------------------------------------------------------------------
            if(onlyPayCustomerQueue.Count > 0)
            {
                // 식당에 가지 않고 바로 퇴실을 희망하는 고객의 경우
                Customer onlyPayCustomer = onlyPayCustomerQueue.Peek().GetComponent<Customer>();

                // 카운터 앞 까지 오지 않으면 계산하지 않기 위해
                if (Vector3.Distance(onlyPayCustomer.transform.position, counterPose[0].position) > 0.1f) continue;

                // 종이 가방을 Active
                GameObject paperBag = paperBagPool.GetObject();
                paperBag.transform.SetPositionAndRotation(paperBagPose.position, Quaternion.Euler(0f, 90f, 0f));

                yield return new WaitForSeconds(0.5f); // 종이 가방 Active시 실행되는 애니메이션을 기다리기 위함

                // 선택한 고객의 스택에 있는 모든 빵들을 빼서 종이 가방에 넣는 코드
                while (onlyPayCustomer.customerStack.Count > 0)
                {
                    var bread = onlyPayCustomer.customerStack.Pop();
                    onlyPayCustomer.CustomerPopAnim(bread, paperBagEntrancePose.position, customerPopAnimTime);
                    yield return new WaitForSeconds(customerPopAnimTime + 0.1f);
                    breadPool.ReturnObject(bread);
                }

                // 종이 가방이 닫히는 애니메이션 실행 & 실행 시간 기다리기
                paperBag.GetComponent<Animator>().SetBool("isBagClose", true);
                yield return new WaitForSeconds(0.6f);

                PayForPlayer();
                onlyPayCustomer.PushCustomerStack(paperBag, 1);
                onlyPayCustomer.SetQuestState(QuestState.Leave);
                onlyPayCustomerQueue.Dequeue();

                UpdateCustomerPositions(0);
            }
           
//-----------------------------------------------------------------------------------------------

            // 식당에 가서 빵을 먹길 원하는 고객의 경우
            if(diningCustomerQueue.Count > 0)
            {
                Customer diningCustomer = diningCustomerQueue.Peek().GetComponent<Customer>();

                if (Vector3.Distance(diningCustomer.transform.position, counterPose[1].position) > 0.1f) continue;

                // 아직 식당이 비활성화인 경우 continue
                if (dining.isActive == false)
                {
                    continue;
                }

                PayForPlayer();
                diningCustomer.SetQuestState(QuestState.EatInRestaurant);
                diningCustomerQueue.Dequeue();

                UpdateCustomerPositions(1);
            }
        }
    }

    public void PayForPlayer()
    {
        // Player에게 돈을 지불하도록 코드 짜기 (5 ~ 15원 랜덤하게)
        int money = Random.Range(5, 16);

        moneyArea.BuildMoneyStack(money);
        audioSource.PlayOneShot(cashSound);
    }

    public void UpdateCustomerPositions(int index) // 줄 서 있는 고객의 위치를 업데이트
    {
        if(index == 0)
        {
            int payCustomerIndex = 0;

            foreach (var c in onlyPayCustomerQueue)
            {
                Customer customer = c.GetComponent<Customer>();
                Vector3 queueStartPoint = counterPose[0].position;
                float zPosition = queueStartPoint.z + payCustomerIndex * distanceBetweenCustomers;

                Vector3 newPosition = new Vector3(queueStartPoint.x, queueStartPoint.y, zPosition);
                customer.SetGoalPose(transform.position, newPosition);

                payCustomerIndex++;
            }
        }
        else if(index == 1)
        {
            int diningCustomerIndex = 0;

            foreach (var c in diningCustomerQueue)
            {
                Customer customer = c.GetComponent<Customer>();

                Vector3 queueStartPoint = counterPose[1].position;
                float zPosition = queueStartPoint.z + diningCustomerIndex * distanceBetweenCustomers;

                Vector3 newPosition = new Vector3(queueStartPoint.x, queueStartPoint.y, zPosition);
                customer.SetGoalPose(transform.position, newPosition);

                diningCustomerIndex++;
            }
        }
    }
}
