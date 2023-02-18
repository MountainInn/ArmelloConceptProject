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

    private void OnIsOngoingSync(bool oldb, bool newb)
    {
        isOngoingReactive.Value = newb;
    }

    CombatView combatView;
    private List<CombatUnit[]> combatList;

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

    public override void OnStartServer()
    {
        MessageBroker.Default
            .Receive<CombatUnit[]>()
            .Subscribe(AddCombatToList);

        FindObjectOfType<TurnSystem>()
            .onRoundEnd += StartAllCombats;
    }

    [Server]
    private void AddCombatToList(CombatUnit[] units)
    {
        combatList.Add(units);
    }

    [Server]
    public void StartAllCombats()
    {
        combatList
            .ForEach(StartCombat);

        combatList.Clear();
    }

    [Server]
    public void StartCombat(params CombatUnit[] units)
    {
        InitCombatViews(units);
        CmdStartCombatSimulation(units);
    }

    [Server]
    private void InitCombatViews(CombatUnit[] units)
    {
        units
            .Select(u => u.netIdentity.connectionToClient)
            .ToList()
            .ForEach(conn => TargetInitCombatViews(conn, units));
    }

    [TargetRpc]
    public void TargetInitCombatViews(NetworkConnectionToClient conn, CombatUnit[] units)
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
            .Where(_ =>
            {
                bool
                    combatEnded = (combatDuration -= Time.deltaTime) <= 0,
                    onlyOneUnitLeftAlive = units.Count(u => u.health > 0) == 1;

                return combatEnded || onlyOneUnitLeftAlive;
            })
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
