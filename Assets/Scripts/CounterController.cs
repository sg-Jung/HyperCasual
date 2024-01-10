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

    public Queue<GameObject> customerQueue = new Queue<GameObject>();

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

    private void Awake() 
    {
        paperBagPool = ManagerSingleton.GetObjectPool<ObjectPool>("PaperBag");
        breadPool = ManagerSingleton.GetObjectPool<ObjectPool>("Bread");
        moneyPool = ManagerSingleton.GetObjectPool<ObjectPool>("Money");
    }

    private void Start() 
    {
        StartCoroutine(PlayerPayingForCustomer());
    }

    private IEnumerator PlayerPayingForCustomer()
    {
        while(true)
        {
            yield return new WaitUntil(() => playerInCounter);
            if(customerQueue.Count <= 0) continue;
            
            yield return new WaitForSeconds(payingTime);

            // customerQueue의 맨 앞에 있는 고객을 선택하고 필요한 데이터를 받는다
            Customer customer = customerQueue.Peek().GetComponent<Customer>();
            bool heWantDining = customer.heWantDining;

            if(!heWantDining) // 식당에 가지 않고 바로 퇴실을 희망하는 고객의 경우
            {
                // 카운터 앞 까지 오지 않으면 계산하지 않기 위해
                if(Vector3.Distance(customer.transform.position, counterPose[0].position) > 0.1f) continue;

                // 종이 가방을 Active
                GameObject paperBag = paperBagPool.GetObject();
                paperBag.transform.SetPositionAndRotation(paperBagPose.position, Quaternion.Euler(0f, 90f, 0f));

                yield return new WaitForSeconds(0.5f); // 종이 가방 Active시 실행되는 애니메이션을 기다리기 위함

                // 선택한 고객의 스택에 있는 모든 빵들을 빼서 종이 가방에 넣는 코드
                while (customer.customerStack.Count > 0)
                {
                    var bread = customer.customerStack.Pop();
                    customer.CustomerPopAnim(bread, paperBagEntrancePose.position, customerPopAnimTime);
                    yield return new WaitForSeconds(customerPopAnimTime + 0.1f);
                    breadPool.ReturnObject(bread);
                }
                
                // 종이 가방이 닫히는 애니메이션 실행 & 실행 시간 기다리기
                paperBag.GetComponent<Animator>().SetBool("isBagClose", true);
                yield return new WaitForSeconds(0.6f);

                audioSource.PlayOneShot(cashSound);
                customer.PushCustomerStack(paperBag, 1);
                customer.SetQuestState(QuestState.Leave);
            }
            else // 식당에 가서 빵을 먹길 원하는 고객의 경우
            {
                if (Vector3.Distance(customer.transform.position, counterPose[1].position) > 0.1f) continue;

                customer.SetQuestState(QuestState.EatInRestaurant);
            }

            // Player에게 돈을 지불하도록 코드 짜기 (5 ~ 15원 랜덤하게)
            int money = Random.Range(5, 16);

            customerQueue.Dequeue();
            UpdateCustomerPositions();
        }
    }

    public void UpdateCustomerPositions()
    {
        int payCustomerIndex = 0;
        int diningCustomerIndex = 0;
        foreach (var c in customerQueue)
        {
            Customer customer = c.GetComponent<Customer>();
            Vector3 queueStartPoint;
            float zPosition;
            if (!customer.heWantDining)
            {
                queueStartPoint = counterPose[0].position;
                zPosition = queueStartPoint.z + payCustomerIndex * distanceBetweenCustomers;
                payCustomerIndex++;
            }
            else
            {
                queueStartPoint = counterPose[1].position;
                zPosition = queueStartPoint.z + diningCustomerIndex * distanceBetweenCustomers;
                diningCustomerIndex++;
            }
            Vector3 newPosition = new Vector3(queueStartPoint.x, queueStartPoint.y, zPosition);
            customer.SetGoalPose(transform.position, newPosition);
        }
    }
}
