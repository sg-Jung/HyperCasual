using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadBoxController : MonoBehaviour
{
    public Queue<GameObject> breadBoxQueue = new Queue<GameObject>();
    public Transform breadParent;
    public Vector3[] breadPose;
    public int maxBreadsCount;

    private Queue<GameObject> customerQueue = new Queue<GameObject>();

    private void Start() 
    {
        StartCoroutine(TakeBreadToCustomer());
    }

    private IEnumerator TakeBreadToCustomer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // 큐에 고객이 있을 경우 빵 제공 및 처리
            if (customerQueue.Count > 0)
            {
                Customer currentCustomer = customerQueue.Peek().GetComponent<Customer>();
                currentCustomer.TakeBreadFromShelf();

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
