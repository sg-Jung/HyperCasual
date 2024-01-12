using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestTween : MonoBehaviour
{
    public Transform startPoint;
    public Transform midPoint;
    public Transform endPoint;
    public float duration = 2f;

    void Start()
    {
        // 시작점, 중간점, 끝점을 사용하여 포물선 동작을 설정합니다.
        // SetParabolicMovement(startPoint.position, midPoint.position, endPoint.position);
    }

    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            SetParabolicMovement(startPoint.position, midPoint.position, endPoint.position);
        }

    }

    void SetParabolicMovement(Vector3 start, Vector3 mid, Vector3 end)
    {
        // 초기 위치를 시작점으로 설정합니다.
        transform.position = start;

        // DOTween을 사용하여 포물선 동작을 설정합니다.
        transform.DOPath(new Vector3[] { start, mid, end }, duration, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.OutQuad);
    }

}
