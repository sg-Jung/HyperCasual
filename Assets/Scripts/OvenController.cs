using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenController : MonoBehaviour
{
    public ObjectPool breadPool;
    public Transform breadEntrance;
    public Transform breadParent;
    public ParticleSystem[] ps;

    public Queue<GameObject> ovenBreads = new Queue<GameObject>();

    public int maxBreadsCount;
    public float spawnTime;
    public float power;

    // Start is called before the first frame update
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
                obj.transform.position = breadEntrance.position;
                obj.transform.SetParent(breadParent);
                obj.GetComponent<Rigidbody>().AddForce(-Vector3.forward * power, ForceMode.Impulse);

                ovenBreads.Enqueue(obj);
                Debug.Log($"OvenBreads: {ovenBreads.Count}");
            }
            else
            {
                foreach(var p in ps) if(p.isPlaying) p.Stop();
            }
        }
    }
}
