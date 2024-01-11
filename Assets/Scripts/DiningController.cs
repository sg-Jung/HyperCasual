using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiningController : MonoBehaviour
{
    [Header("Object")]
    public GameObject ActiveDining;
    public GameObject DeActiveDining;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clearSound;

    public bool isActive;

    private LockMoneyAreaManager lockMoneyAreaManager;

    private void Start() 
    {
        lockMoneyAreaManager = ManagerSingleton.GetLockMoneyAreaManager();
        StartCoroutine(WaitingLock());
    }

    private IEnumerator WaitingLock()
    {
        // currentLockSequence가 First인 동안 대기, First가 아니게 되면 넘어감
        yield return new WaitWhile(() => lockMoneyAreaManager.currentLockSequence == LockSequence.First);
        isActive = true;
        ActiveDining.SetActive(true);
        DeActiveDining.SetActive(false);

        // lockMoneyArea.transform.localPosition = nextLockMoneyArea;
        // lockMoneyArea.SetNeedMoney(nextLockMoney);
        // lockMoneyArea.SetActiveAndLock(true);

        // yield return new WaitUntil(() => !lockMoneyArea.isLock);
    }
}
