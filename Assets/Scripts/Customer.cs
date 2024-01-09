using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public enum QuestState
    {
        TakeBread,
        PayAtCheckout,
        EatInRestaurant,
        Leave
    }

    public Stack<GameObject> customerStack = new Stack<GameObject>();
    public int neededBreadCount;
    public float moveAnimDuration;

    public bool isStacking;
    public bool isMove;
    public bool isTalking;
    
    private Animator anim;
    private NavMeshAgent nav;
    private QuestState currentQuestState;

    void Start()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        SetQuestState(QuestState.TakeBread);
    }

    void Update()
    {
        CustomerState();
        AnimationSelect();
    }

    private void AnimationSelect()
    {
        if (anim == null) return;
        isStacking = customerStack.Count > 0 ? true : false;

        anim.SetBool("isMove", isMove);
        anim.SetBool("isStack", isStacking);
        anim.SetBool("isTalking", isTalking);
    }

    void SetQuestState(QuestState newState) // 상태 변경 시 한 번 수행
    {
        currentQuestState = newState;
        Debug.Log("퀘스트 상태 변경: " + newState);

        // 상태에 따른 동작 수행
        if (newState == QuestState.TakeBread)
        {
            Debug.Log("빵을 가져가세요.");
            int random = Random.Range(1, 4);
            neededBreadCount = random;
            // 말풍선에 neededBreadCount갯수 넣는 코드 추가

            isMove = true;
        }
        else if (newState == QuestState.PayAtCheckout)
        {
            Debug.Log("계산대에서 계산하세요.");
        }
        else if (newState == QuestState.EatInRestaurant)
        {
            Debug.Log("식당에서 식사하세요.");
        }
        else if (newState == QuestState.Leave)
        {
            Debug.Log("식당을 떠나세요.");
        }
    }

    void CustomerState()
    {
        switch (currentQuestState)
        {
            case QuestState.TakeBread:
                
                break;

    
            case QuestState.PayAtCheckout:
                if (Input.GetKeyDown(KeyCode.P))
                {
                    SetQuestState(QuestState.EatInRestaurant);
                }
                break;

            case QuestState.EatInRestaurant:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    SetQuestState(QuestState.Leave);
                }
                break;

            case QuestState.Leave:
                if (Input.GetKeyDown(KeyCode.L))
                {
                    Debug.Log("퀘스트 완료: 빵을 가져가고, 계산하고, 식당에서 먹고 나가기");
                }
                break;
        }
    }

    public void TakeBreadFromShelf()
    {
        // 빵을 진열대에서 가져가는 애니메이션 구현(iTween)

        Debug.Log("빵을 진열대에서 가져갑니다.");
        DecreaseNeededBreadCount();

        if(neededBreadCount <= 0) SetQuestState(QuestState.PayAtCheckout);  // 다음 퀘스트로 넘어가기
    }

    public void DecreaseNeededBreadCount()
    {
        neededBreadCount = Mathf.Max(0, neededBreadCount - 1);

        // 고객의 말풍선 안의 빵 갯수 감소하도록 구현

    }
}
