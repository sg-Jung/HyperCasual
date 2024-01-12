using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public CameraController cameraController;
    public PlayerMoveJoystick joyStick;
    public PlayerController player;

    public Transform tempTarget;

    private CounterController counter;
    private DiningController dining;
    private LockMoneyAreaManager lockMoneyAreaManager;

    void Start()
    {
        counter = EventObjectsSingleton.GetCounterController();
        dining = EventObjectsSingleton.GetDiningController();
        lockMoneyAreaManager = ManagerSingleton.GetLockMoneyAreaManager();
        StartCoroutine(PlayerQuestCoroutine());
    }

    private IEnumerator PlayerQuestCoroutine()
    {
        yield return new WaitUntil(() => player.playerInCounter && counter.moneyArea.moneyActive);
        StartCoroutine(CameraMoveCoroutine(dining.tablePose));
        
        yield return new WaitUntil(() => dining.isActive);
        StartCoroutine(CameraMoveCoroutine(tempTarget));
        
    }

    private IEnumerator CameraMoveCoroutine(Transform target)
    {
        yield return new WaitForSeconds(2f);

        joyStick.SetMovingLock(true);
        cameraController.SetCameraLockAndNewTarget(true, target);

        yield return new WaitForSeconds(3f);
        joyStick.SetMovingLock(false);
        cameraController.SetCameraLockAndNewTarget(false);
    }
}
