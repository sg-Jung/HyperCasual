using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카운터 옆 
// startPose 4, 0.5, 8.2
// offset 0.8 0.25 -0.6

public class MoneyArea : MonoBehaviour
{
    public Transform startPose;
    public Vector3 offset;
    public Transform moneyParent;
    public int money = 0;
    public bool moneyActive;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cashSound;

    private ObjectPool moneyPool;

    private void Start() 
    {
        moneyPool = ManagerSingleton.GetObjectPool<ObjectPool>("Money");
    }

    void BuildMoneyRemove()
    {
        if(moneyParent.childCount <= 0) return;

        Transform[] children = new Transform[moneyParent.childCount];
        for (int i = 0; i < moneyParent.childCount; i++) 
            children[i] = moneyParent.GetChild(i);

        foreach (Transform child in children)
            moneyPool.ReturnObject(child.gameObject);
    }

    public void BuildMoneyStack(int moneyCount)
    {
        BuildMoneyRemove();
        money += moneyCount;
        moneyActive = true;
        
        int stackHeight = money / 9 + 1;
        int moneyIndex = 0;

        for (int height = 0; height < stackHeight; height++)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if(moneyIndex >= money) return; // money 갯수만큼 생성되면 함수종료

                    Vector3 position = startPose.position + new Vector3(col * offset.x, height * offset.y, row * offset.z);
                    Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

                    var moneyObj = moneyPool.GetObject();
                    moneyObj.transform.SetParent(moneyParent);
                    moneyObj.transform.SetPositionAndRotation(position, rotation);
                    moneyIndex++;
                }
            }
        }
    }

    public void PlayerGetMoney(PlayerController player)
    {
        // StartCoroutine(GetMoney(player));
        player.SetMoney(money);
        audioSource.PlayOneShot(cashSound);

        BuildMoneyRemove();
        money = 0;
        moneyActive = false;
        Debug.Log($"Player 돈 획득: {player.curMoney}");
    }


    // 포물선을 그리며 돈을 먹도록 나중에 구현해볼 것
    private IEnumerator GetMoney(PlayerController player)
    {
        Transform[] children = new Transform[moneyParent.childCount];
        for (int i = 0; i < moneyParent.childCount; i++)
            children[i] = moneyParent.GetChild(i);

        foreach (Transform child in children)
        {
            yield return new WaitForSeconds(0.05f);
            moneyPool.ReturnObject(child.gameObject);
        }

        player.curMoney += money;
        money = 0;
    }

    private void OnTriggerStay(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            if (money > 0)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                PlayerGetMoney(player);
            }
        }
    }
}
