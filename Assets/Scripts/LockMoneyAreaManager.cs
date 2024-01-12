using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LockSequence{ First, Second, Third}
public class LockMoneyAreaManager : MonoBehaviour
{
    public LockSequence currentLockSequence;
    public List<GameObject> curLockArea = new List<GameObject>();
    public Vector3 lockAreaRotate;
    public int maxLevel;
    public int curLevel;

    [Header("First")]
    public List<Vector3> firstLockAreaPose;
    public List<int> firstLockAreaMoney;

    [Header("Second")]
    public List<Vector3> secondLockAreaPose;
    public List<int> secondLockAreaMoney;

    private Dictionary<int, List<Vector3>> lockAreaPoseDict = new Dictionary<int, List<Vector3>>();
    private Dictionary<int, List<int>> lockAreaMoneyDict = new Dictionary<int, List<int>>();
    
    private ObjectPool lockMoneyPool;

    private void Start() 
    {
        lockMoneyPool = ManagerSingleton.GetObjectPool<ObjectPool>("LockMoneyArea");
        PutDictionary();
        SetLockSequence(LockSequence.First);
    }

    void Update()
    {
        CustomerState();
    }

    private void PutDictionary()
    {
        lockAreaPoseDict.Add(1, firstLockAreaPose);
        lockAreaMoneyDict.Add(1, firstLockAreaMoney);

        lockAreaPoseDict.Add(2, secondLockAreaPose);
        lockAreaMoneyDict.Add(2, secondLockAreaMoney);
    }

    private void UpdateLockMoneyAreaPlace(int level)
    {
        List<Vector3> lockAreaPose = lockAreaPoseDict[level];
        List<int> lockAreaMoney = lockAreaMoneyDict[level];

        for(int i = 0; i < lockAreaPose.Count; i++)
        {
            var lockArea = lockMoneyPool.GetObject();
            LockMoneyArea lockMoneyArea = lockArea.GetComponent<LockMoneyArea>();
            lockArea.transform.SetPositionAndRotation(lockAreaPose[i], Quaternion.Euler(lockAreaRotate));
            lockMoneyArea.InitLockMoneyArea(lockAreaMoney[i]);

            curLockArea.Add(lockArea);
        }
    }

    public void SetLockSequence(LockSequence newState) // 상태 변경 시 한 번 수행
    {
        currentLockSequence = newState;
        Debug.Log("퀘스트 상태 변경: " + newState);

        // 상태에 따른 동작 수행
        if (newState == LockSequence.First)
        {
            curLevel = 1;
            UpdateLockMoneyAreaPlace(curLevel);
        }
        else if (newState == LockSequence.Second)
        {
            curLevel = 2;
            UpdateLockMoneyAreaPlace(curLevel);
        }
        else if (newState == LockSequence.Third)
        {
            curLevel = 3;

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
                if (curLockArea.Count <= 0)
                {
                    SetLockSequence(LockSequence.Third);
                }
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

    // 잠금해제 사운드 및 파티클 동작완료 후 반환하기 위해 코루틴 사용
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
