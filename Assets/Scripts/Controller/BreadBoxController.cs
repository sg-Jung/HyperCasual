using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreadBoxController : MonoBehaviour
{
    public Queue<GameObject> breadBoxQueue = new Queue<GameObject>();
    public Transform breadParent;
    public Transform customerParent;
    public Vector3[] breadPose;
    public Vector3[] customerPose;
    public int maxBreadsCount;
    public float takeDurationTime;
    [HideInInspector] public int breadPoseIndex = 0;
    [HideInInspector] public bool isPlayerPutBread;

    private Queue<GameObject> customerQueue = new Queue<GameObject>();

    private void Start() 
    {
        StartCoroutine(TakeBreadToCustomer());
    }

    private IEnumerator TakeBreadToCustomer()
    {
        while (true)
        {
            yield return new WaitForSeconds(takeDurationTime);
            // 플레이어가 빵을 놓는 중이라면 빵을 가져갈 수 없음
            if(isPlayerPutBread) continue;

            // 큐에 고객이 있을 경우 빵 제공 및 처리
            if (customerQueue.Count > 0)
            {
                if(breadBoxQueue.Count <= 0) continue;

                // 고객 큐의 맨 앞 고객을 선택
                Customer currentCustomer = customerQueue.Peek().GetComponent<Customer>();

                // 고객이 필요한 빵이 0보다 크고, 빵이 아직 있다면 takeDurationTime마다 하나 씩 빵을 가져감
                // 빵을 가져가는 도중 빵 진열대에 빵이 없다면 반복문 탈출
                while(currentCustomer.neededBreadCount > 0)
                {
                    if(breadBoxQueue.Count <= 0) break;
                    yield return new WaitForSeconds(takeDurationTime);
                    currentCustomer.PushCustomerStack(breadBoxQueue.Dequeue(), 0);
                }

                // 현재 고객이 원하는 만큼 빵을 가져갔다면 다음 상태로 이동하기 위해 고객 큐에서 빼준다
                if (currentCustomer.neededBreadCount <= 0)
                    customerQueue.Dequeue();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Customer"))
        {
            Customer customer = other.GetComponent<Customer>();
            if (customer != null)
            {
                // 고객이 BreadBox 주위의 BoxCollider에 Trigger되고, 필요한 빵 갯수가 0 이상이라면 Queue에 추가
                if(customer.neededBreadCount > 0)
                    customerQueue.Enqueue(customer.gameObject);
            }
        }
    }
}
