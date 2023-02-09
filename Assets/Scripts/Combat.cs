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

    [SyncVar(hook=nameof(OnIsOngoingSync))]
    public bool isOngoing;
    public ReactiveProperty<bool> isOngoingReactive = new ReactiveProperty<bool>(false);

    CombatView combatView;


    private void OnIsOngoingSync(bool oldb, bool newb)
    {
        isOngoingReactive.Value = newb;
    }

    [Inject]
    public void Construct(CombatView combatView)
    {
        this.combatView = combatView;
    }

    public override void OnStartClient()
    {
        isOngoingReactive
            .Subscribe(combatView.SetVisible);
    }


    [Command(requiresAuthority=false)]
    public void CmdStartCombat(params CombatUnit[] units)
    {
        RpcInitCombatViews(units);

        CmdStartCombatSimulation(units);
    }

    [ClientRpc]
    public void RpcInitCombatViews(params CombatUnit[] units)
    {
        combatView.InitStatsView(units);
    }

    [Command(requiresAuthority=false)]
    public void CmdStartCombatSimulation(params CombatUnit[] units)
    {
        CompositeDisposable combatDisposables = new CompositeDisposable();

        this.UpdateAsObservable()
            .SelectMany(_ => units)
            .Where(u => u.AttackTimerTick(Time.deltaTime))
            .Subscribe(u =>
            {
                var target =
                    units
                    .NotEqual(u)
                    .Where(t => t.health > 0)
                    .GetRandom();

                var damage =
                    (int)Mathf.Max(1, u.attack / target.defense );

                target.health -= damage;
            })
            .AddTo(combatDisposables);

        float combatDuration = this.combatDurationInSeconds;

        this.UpdateAsObservable()
            .Where(_ => (combatDuration -= Time.deltaTime) <= 0)
            .Subscribe(_ =>
            {
                isOngoing = false;
                combatDisposables.Dispose();
            })
            .AddTo(combatDisposables);

        isOngoing = true;
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
