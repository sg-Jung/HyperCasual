using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenController : MonoBehaviour
{
    public Transform breadEntrance;
    public Transform breadParent;
    public ParticleSystem[] ps;

    public Queue<GameObject> ovenBreads = new Queue<GameObject>();

    public int maxBreadsCount;
    public float spawnTime;
    public float power;
    
    private ObjectPool breadPool;

    private void Awake() 
    {
        breadPool = ManagerSingleton.GetObjectPool<ObjectPool>("Bread");
    }

    void Start()
    {
        StartCoroutine(SpawnBreads());
    }
    
    private IEnumerator SpawnBreads()
    {
        while(true)
        {
            yield return new WaitForSeconds(spawnTime);
            if (ovenBreads.Count < maxBreadsCount)
            {
                foreach (var p in ps) if(!p.isPlaying) p.Play();

                var obj = breadPool.GetObject();
                obj.transform.SetParent(breadParent);
                var bread = obj?.GetComponent<Bread>();
                bread?.SetBreadColliderRigidBodyEnable(true);

                obj.transform.SetPositionAndRotation(breadEntrance.position, Quaternion.identity);
                obj.GetComponent<Rigidbody>().AddForce(-Vector3.forward * power, ForceMode.Impulse);

                ovenBreads.Enqueue(obj);
            }
            else
            {
                foreach(var p in ps) if(p.isPlaying) p.Stop();
            }
        }
    }
}
