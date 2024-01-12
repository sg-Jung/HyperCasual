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
    public float animDuration;

    [Header("Audio")]
    public AudioSource audioSource;

    private ObjectPool moneyPool;
    private Coroutine getMoneyCor;

    private void Start() 
    {
        moneyPool = ManagerSingleton.GetObjectPool<ObjectPool>("Money");
    }

    public void PayForPlayer()
    {
        // Player에게 돈을 지불하도록 코드 짜기 (5 ~ 15원 랜덤하게)
        int money = Random.Range(5, 16);

        BuildMoneyStack(money);
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

    // 포물선을 그리며 돈을 먹도록 나중에 구현해볼 것
    private IEnumerator GetMoney(PlayerController player)
    {
        moneyActive = false;

        Transform[] children = new Transform[moneyParent.childCount];
        for (int i = 0; i < moneyParent.childCount; i++)
            children[i] = moneyParent.GetChild(i);

        foreach (Transform child in children)
        {
            child.SetParabolicMovement(player.animMidPose.position, player.animEndPose.position, animDuration);
            yield return new WaitForSeconds(animDuration + (animDuration / 2));
            if(!audioSource.isPlaying) audioSource.Play();
            moneyPool.ReturnObject(child.gameObject);
        }

        player.SetMoney(money);
        money = 0;
        Debug.Log($"Player 돈 획득: {player.curMoney}");
    }

    private void OnTriggerStay(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            if (money > 0 && moneyActive)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                // PlayerGetMoney(player);
                if (getMoneyCor == null)
                    StartCoroutine(GetMoney(player));
            }
        }
    }
}
