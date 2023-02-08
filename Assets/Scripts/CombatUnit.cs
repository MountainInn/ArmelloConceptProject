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

    IDisposable battleDisposable;

    [Client]
    public void StartSimulatingBattleObservable(Hit[] hits)
    {
        battleDisposable =
            this.UpdateAsObservable()
            .Where(_ =>
                   hits.Any() &&
                   AttackTimerTick(Time.deltaTime))
            .Subscribe(_ =>
            {
                var hit = hits.First();
                hits = hits.Skip(1).ToArray();

                hit.target.healthReactive.Value -= hit.damage;

                Debug.Log($"Hit for {hit.damage}");

                if (!hits.Any())
                {
                    Debug.Log($"Run Out of Hits");
                    battleDisposable.Dispose();
                }
            });
    }

    [Client]
    public bool AttackTimerTick(float delta)
    {
        attackTimerRatioReactive.Value += speed / 100f * delta;

        bool res = attackTimerRatioReactive.Value >= 1f;

        if (res)
            attackTimerRatioReactive.Value -= 1f;

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
