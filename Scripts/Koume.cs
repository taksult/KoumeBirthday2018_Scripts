using System;
using System.Collections.Generic;
using UnityEngine;


public class Koume : MonoBehaviour
{
    public enum StressPhase
    {
        Happy = 0,
        Fine = 1,
        Annoyed = 2,
        Fear = 3
    }

    public static readonly float StressStep = 0.1f;
    public static readonly float StressMin = 0;
    public static readonly float StressMax = 40;

    public float Stress { get; set; } = 10.1f;
    public float updateInterval = 1.0f;
    private float lastUpdate;

    private void Start()
    {
        lastUpdate = Time.time;
        GameObject.Find("face").GetComponent<Animator>().SetFloat("Stress", Stress);
    }

    private void FixedUpdate()
    {
        if (Time.time - lastUpdate > updateInterval)
        {
            updateStress();
        }
    }

    // ストレス値更新
    private void updateStress()
    {
        lastUpdate = Time.time;
        KoumeMove kMove = GetComponent<KoumeMove>();
        switch(kMove.Mode)
        {
            case KoumeMove.MoveMode.Default :
                IncreaseStress(StressStep);
                break;

            case KoumeMove.MoveMode.Chasing :
            case KoumeMove.MoveMode.ChaseBuf :
                IncreaseStress(StressStep * 5);
                break;

            case KoumeMove.MoveMode.Lost:
                IncreaseStress(StressStep * 25);
                break;

            default: break;
        }

        GameObject.Find("face").GetComponent<Animator>().SetFloat("Stress", Stress);
    }

    public void IncreaseStress(float v)
    {
        if (Stress < StressMax)
        {
            ChangeStress(v);
        }
    }

    public void ReduceStress(float v)
    {
        if (Stress > StressMin)
        {
            ChangeStress(-v);
        }
    }

    // 変動量をストレスに加算
    private void ChangeStress(float v)
    {
        Stress += v;

        // 最大/最小値より大きく/小さくならない
        if (Stress < StressMin)
        {
            Stress = StressMin;
        }
        else if (Stress > StressMax)
        {
            Stress = StressMax;
        }
        // ストレスモニターのアニメーションパラメータを更新
        GameObject.Find("face").GetComponent<Animator>().SetFloat("Stress", Stress);
    }
}

