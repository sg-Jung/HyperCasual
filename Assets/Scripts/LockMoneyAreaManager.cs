using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LockSequence{ First, Second, Third}
public class LockMoneyAreaManager : MonoBehaviour
{
    public LockSequence currentLockSequence;
    public List<GameObject> curLockArea = new List<GameObject>();

    [Header("First")]
    public Vector3 firstLockAreaPose;
    public Vector3 firstLockAreaRotate;
    public int firstLockAreaMoney;

    [Header("Second")]

    
    private ObjectPool lockMoneyPool;

    private void Start() 
    {
        lockMoneyPool = ManagerSingleton.GetObjectPool<ObjectPool>("LockMoneyArea");
        SetLockSequence(LockSequence.First);
    }

    void Update()
    {
        CustomerState();
    }

    public void SetLockSequence(LockSequence newState) // 상태 변경 시 한 번 수행
    {
        currentLockSequence = newState;
        Debug.Log("퀘스트 상태 변경: " + newState);

        // 상태에 따른 동작 수행
        if (newState == LockSequence.First)
        {
            var lockArea = lockMoneyPool.GetObject();
            LockMoneyArea lockMoneyArea = lockArea.GetComponent<LockMoneyArea>();

            lockArea.transform.SetPositionAndRotation(firstLockAreaPose, Quaternion.Euler(firstLockAreaRotate));
            lockMoneyArea.InitLockMoneyArea(firstLockAreaMoney);

            curLockArea.Add(lockArea);
        }
        else if (newState == LockSequence.Second)
        {

        }
        else if (newState == LockSequence.Third)
        {

        }
       
    }

    void CustomerState()
    {
        switch (currentLockSequence)
        {
            case LockSequence.First:
            // curLockArea가 0보다 작거나 같다면 현재 단계에서 사용된 LockArea가 모두 잠금이 풀렸다는 뜻이므로 다음단계로 넘어감
                if (curLockArea.Count <= 0) 
                {
                    SetLockSequence(LockSequence.Second);
                }
                break;

            case LockSequence.Second:
                break;

            case LockSequence.Third:
                
                break;
        }

        CheckCurLockMoneyAreaIsUnLock();
    }

    // 현재 사용중인 LockMoneyArea중 잠금이 풀린게 있다면 다시 pool로 반환
    private void CheckCurLockMoneyAreaIsUnLock()
    {
        if(curLockArea.Count <= 0) return;

        for (int i = 0; i < curLockArea.Count; i++)
        {
            LockMoneyArea lockMoneyArea = curLockArea[i].GetComponent<LockMoneyArea>();
            if (lockMoneyArea.isLock) 
                continue;
            else
            {
                StartCoroutine(CurLockMoneyAreaReturn(curLockArea[i]));
            }
        }
    }

    private IEnumerator CurLockMoneyAreaReturn(GameObject lockArea)
    {
        curLockArea.Remove(lockArea);
        yield return new WaitForSeconds(1.2f);
        lockMoneyPool.ReturnObject(lockArea);
    }

    private void CurLockMoneyAreaAllReturn()
    {
        foreach (var lockArea in curLockArea)
        {
            lockMoneyPool.ReturnObject(lockArea);
        }
    }
}
