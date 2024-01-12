using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNavigation : MonoBehaviour
{
    public PlayerController player;
    public GameObject playerArrow;
    public GameObject objectArrow;

    public Transform ovenTransform;
    public Transform breadBoxTransform;
    public Transform counterTransform;
    public Transform moneyAreaTransform;
    public Transform diningTransform;

    public float arrowDistance;
    public float objectY;
    public float moneyY;
    public float tableY;

    private Vector3 goalPose;
    private CounterController counter;
    private DiningController dining;

    void Start()
    {
        counter = EventObjectsSingleton.GetCounterController();
        dining = EventObjectsSingleton.GetDiningController();
        StartCoroutine(PlayerFollowArrow());
    }

    private void Update() 
    {
        PlayerArrowPositionRotation();
    }

    private void PlayerArrowPositionRotation()
    {
        if(!playerArrow.activeInHierarchy) return;

        // 플레이어를 기준으로 목표지점의 방향을 구하고 Normalize
        Vector3 directionToTarget = goalPose - player.transform.position;
        directionToTarget.y = 0f;
        directionToTarget.Normalize();

        // 플레이어 주위의 화살표를 목표지점의 방향으로 플레이어에서 arrowDistance만큼 떨어진 곳으로 위치시킴
        Vector3 arrowPosition = player.transform.position + directionToTarget * arrowDistance;
        playerArrow.transform.position = arrowPosition;

        // 화살표가 목표지점을 가리키도록 회전
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);
        playerArrow.transform.rotation = rotationToTarget;
    }


    public void SetGoalPose(Vector3 goalPose, float y)
    {
        this.goalPose = goalPose;
        objectArrow.transform.position = goalPose + new Vector3(0f, y, 0f);
    }

    private IEnumerator PlayerFollowArrow()
    {
        SetGoalPose(ovenTransform.position, objectY);
        yield return new WaitUntil(() => player.playerInOven);

        SetGoalPose(breadBoxTransform.position, objectY);
        yield return new WaitUntil(() => counter.onlyPayCustomerQueue.Count > 0);

        SetGoalPose(counterTransform.position, objectY);
        yield return new WaitUntil(() => player.playerInCounter && counter.moneyArea.moneyActive);

        SetGoalPose(moneyAreaTransform.position, moneyY);
        yield return new WaitUntil(() => !counter.moneyArea.moneyActive);

        SetGoalPose(diningTransform.position, tableY);
        yield return new WaitUntil(() => dining.isActive);
       
        playerArrow.SetActive(false);
        objectArrow.SetActive(false);
    }
}
