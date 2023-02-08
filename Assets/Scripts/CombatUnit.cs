using UnityEngine;
using Mirror;
using System.Linq;
using System;
using UniRx;
using UniRx.Triggers;

public class CombatUnit : NetworkBehaviour
{
    public ReactiveProperty<int>
        healthReactive = new ReactiveProperty<int>(0);

    public ReactiveProperty<float>
        attackTimerRatioReactive = new ReactiveProperty<float>(0);

    [SyncVar(hook = nameof(OnHealthSync))]
    public int health;

    [SyncVar(hook = nameof(OnAttackTimerRatioSync))]
    public float attackTimerRatio;

    [SyncVar]
    public int
        defense,
        attack,
        speed;

    public bool AttackTimerTick(float delta)
    {
        attackTimerRatio += speed / 100f * delta;

        bool res = attackTimerRatio >= 1f;

        if (res)
            attackTimerRatio -= 1f;

        return res;
    }


    private void OnAttackTimerRatioSync(float oldR, float newR)
    {
        attackTimerRatioReactive.Value = newR;
    }

    private void OnHealthSync(int oldH, int newH)
    {
        healthReactive.Value = newH;
    }

}
