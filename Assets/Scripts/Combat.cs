using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using UniRx.Triggers;
using MountainInn;
using Zenject;

public class Combat : NetworkBehaviour
{
    [SerializeField]
    float combatDurationInSeconds = 5;

    IDisposable
        battleSubscription;

    public ReactiveProperty<bool>
        isOngoing = new ReactiveProperty<bool>(false);

    CombatView combatView;

    [Inject]
    public void Construct(CombatView combatView)
    {
        this.combatView = combatView;

        isOngoing
            .Subscribe(b => {
                combatView.SetVisible(b);
            });
    }

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
                    u.attackTimerRatio +
                    combatDurationInSeconds * (u.speed / 100f);

                int fullAttacks =
                    (int)MathF.Floor(attackPerBattle);

                u.attackTimerRatio = attackPerBattle - fullAttacks;

                var hits =
                    fullAttacks.ToRange()
                    .Select(n => {
                        var target = units.NotEqual(u).GetRandom();

                        var damage =
                            (int)Mathf.Max(1, u.attack / target.defense );

                        target.health -= damage;
                       
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
        var units = hitsAndTargets.Select(log => log.unit).ToArray();

        combatView.InitStatsView(units);

        hitsAndTargets
            .ToList()
            .ForEach(log => log.unit.StartSimulatingBattleObservable(log.hits));

        isOngoing.Value = true;
    }

    [Client]
    public void EndCombat()
    {
        battleSubscription?.Dispose();

        isOngoing.Value = false;
    }
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
