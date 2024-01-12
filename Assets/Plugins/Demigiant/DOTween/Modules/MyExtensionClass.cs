using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class MyExtensionClass
{
    public static void SetParabolicMovement(this Transform target, Vector3 start, Vector3 mid, Vector3 end, float duration)
    {
        // start를 시작점으로 설정
        target.position = start;

        target.DOPath(new Vector3[] { start, mid, end }, duration, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.OutQuad);
    }

    // 현재 위치에서 바로 시작되는 애니메이션
    public static void SetParabolicMovement(this Transform target, Vector3 mid, Vector3 end, float duration)
    {
        target.DOPath(new Vector3[] { target.position, mid, end }, duration, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.OutQuad);
    }
}
