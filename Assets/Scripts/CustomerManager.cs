using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public List<GameObject> currentCustomers;
    public Transform customerEntrance;
    public Transform customerParent;

    public int maxCurrentCustomerCount;
    public float spawnTime;

    private ObjectPool customerPool;

    private void Awake() 
    {
        customerPool = ManagerSingleton.GetObjectPool<ObjectPool>("Customer");
    }

    void Start()
    {
        StartCoroutine(SpawnCustomers());
    }

    private IEnumerator SpawnCustomers()
    {
        while (true)
        {
            int currentCustomerCount = customerPool.usingObject.Count;
            int customersToSpawn = Mathf.Min(maxCurrentCustomerCount - currentCustomerCount, Random.Range(2, 4));
            
            for (int i = 0; i < customersToSpawn; i++)
            {
                yield return new WaitForSeconds(spawnTime);

                GameObject obj = customerPool.GetObject();
                obj.transform.SetParent(customerParent);
                obj.transform.position = customerEntrance.position;

                BreadBoxController breadBoxController = EventObjectsSingleton.GetBreadBoxController();
                Vector3[] customerPose = breadBoxController.customerPose;

                Vector3 customerBreadBoxPose = customerPose[i];

                Customer customer = obj.GetComponent<Customer>();
                customer.SetAllUIActive(false);
                customer.goalPose = customerBreadBoxPose;
                customer.SetQuestState(QuestState.TakeBread);

                currentCustomers.Add(obj);
            }

            yield return new WaitUntil(() => IsCurSpawnCstomerEndFirstQuest());
        }
    }

    private bool IsCurSpawnCstomerEndFirstQuest()
    {
        // 이번 루프 때 스폰된 고객들이 아직도 빵 가져가기 퀘스트를 진행중이라면 false, 전부 수행 완료했으면 true
        foreach(var obj in currentCustomers)
        {
            if(obj.GetComponent<Customer>().currentQuestState == QuestState.TakeBread)
                return false;
        }

        return true;
    }

}
