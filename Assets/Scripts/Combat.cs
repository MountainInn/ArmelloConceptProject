using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using UniRx.Triggers;
using MountainInn;

public class Combat : NetworkBehaviour
{
    [SerializeField]
    float combatDurationInSeconds = 5;

    IDisposable
        battleSubscription;

    public ReactiveProperty<bool>
        isOngoing = new ReactiveProperty<bool>(false);

   
    [Server]
    public void SrvStartCombat(params CombatUnit[] units)
    {
        var hitsAndTargets = SrvSimulateCombat(units);

        RpcSimulateCombat(hitsAndTargets);
    }

    [Server]
    public HitLog[] SrvSimulateCombat(params CombatUnit[] units)
    {
        var attacks =
            units.Select(u => {
                float attackPerBattle =
                    u.attackTimerRatio.Value +
                    combatDurationInSeconds * (u.speed / 100f);

                int fullAttacks =
                    (int)MathF.Floor(attackPerBattle);

                u.attackTimerRatio.Value = attackPerBattle - fullAttacks;

                var hits =
                    fullAttacks.ToRange()
                    .Select(n => {
                        var target = units.NotEqual(u).GetRandom();

                        var damage =
                            (int)Mathf.Max(1, u.attack / target.defense );
                       
                        return new Hit(){ target = target, damage = damage };
                    })
                    .ToArray();

                return new HitLog(){ unit = u, hits = hits };
            })
            .ToArray();

        return attacks;
    }

    [TargetRpc]
    private void RpcSimulateCombat(HitLog[] hitsAndTargets)
    {
        isOngoing.Value = true;
    }

    [Client]
    public void EndCombat()
    {
        battleSubscription?.Dispose();

        isOngoing.Value = false;
    }

    public struct HitLog
    {
        public  CombatUnit unit;
        public  Hit[] hits;
    }

    public struct Hit
    {
        public CombatUnit
            target;
        public int
            damage;
    }

    public class CombatUnit : NetworkBehaviour
    {
        public ReactiveProperty<int>
            health = new ReactiveProperty<int>(0);

        public ReactiveProperty<float>
            attackTimerRatio = new ReactiveProperty<float>(0);

        [SyncVar]
        public int
            defense,
            attack,
            speed;

        IDisposable battleDisposable;

        [Client]
        public void SimulateBattle(Hit[] hits, float delta)
        {
            battleDisposable =
                this.UpdateAsObservable()
                .Select(_ =>
                        hits.Any() &&
                        AttackTimerTick(delta))
                .Where(b => b == true)
                .Subscribe(_ =>
                {
                    var hit = hits.First();
                    hit.target.health.Value -= hit.damage;
                    hits = hits.Skip(1).ToArray();

                    if (!hits.Any())
                        battleDisposable.Dispose();
                });
        }

        [Client]
        public bool AttackTimerTick(float delta)
        {
            attackTimerRatio.Value += speed / 100f * delta;

            bool res = attackTimerRatio.Value >= 1f;

            if (res)
                attackTimerRatio.Value -= 1f;

            return res;
        }
    }
}
