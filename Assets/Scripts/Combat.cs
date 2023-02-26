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
    [SerializeField] int warPhaseStartRound = 6;
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
    private AttackMarkerPool attackMarkerPool;
    private List<LineRenderer> attackMarkers;


    [Inject]
    public void Construct(CombatView combatView)
    {
        this.combatView = combatView;
    }

    public override void OnStartClient()
    {
        isOngoingReactive
            .Subscribe(combatView.SetVisible);

        attackMarkerPool = new AttackMarkerPool(Resources.Load<LineRenderer>("Prefabs/Attack Marker"));
        attackMarkers = new List<LineRenderer>();
    }

    public override void OnStartServer()
    {
        combatList = new List<CombatUnit[]>();
       
        MessageBroker.Default
            .Receive<CombatUnit[]>()
            .Subscribe(AddCombatToList)
            .AddTo(this);

        MessageBroker.Default
            .Receive<TurnSystem.OnRoundEnd>()
            .Subscribe(WarPhase)
            .AddTo(this);
    }

    [Server]
    private void WarPhase(TurnSystem.OnRoundEnd msg)
    {
        if (msg.roundCount >= warPhaseStartRound)
        {
            var allUnits = FindObjectsOfType<CombatUnit>();

            if (allUnits.Count() < 2)
                return;

            AddCombatToList(allUnits);

            StartAllCombats();
        }
    }

    [Server]
    private void AddCombatToList(CombatUnit[] units)
    {
        if (combatList.Any(c => units.All(u => c.Contains(u))))
            return;
       
        combatList.Add(units);

        var attackerConn = units[0].netIdentity.connectionToClient;
        TargetSpawnAttackMarker(attackerConn, units);

        StartAllCombats();
    }

    [TargetRpc]
    private void TargetSpawnAttackMarker(NetworkConnectionToClient conn, CombatUnit[] units)
    {
        var attackMarker = attackMarkerPool.Rent();

        var positions =
            units.Select(u => u.transform.position).ToArray();

        attackMarker.SetPositions(positions);

        attackMarkers.Add(attackMarker);
    }

    [Server]
    public void StartAllCombats()
    {
        combatList
            .ForEach(StartCombat);

        combatList.Clear();

        RpcDespawnAttackMarkers();
    }

    [ClientRpc]
    private void RpcDespawnAttackMarkers()
    {
        if (!attackMarkers.Any())
            return;

        attackMarkers.ForEach(attackMarkerPool.Return);
        attackMarkers.Clear();
    }

    [Server]
    public void StartCombat(CombatUnit[] units)
    {
        var hits = SimulateCombat(units.ToList());

        units
            .Select(u => u.netIdentity.connectionToClient)
            .Distinct()
            .ToList()
            .ForEach(conn => TargetInitCombatViews(conn, units, hits));
    }

    [TargetRpc]
    public void TargetInitCombatViews(NetworkConnection target, CombatUnit[] units, Hit[] hits)
    {
        combatView.InitCombatView(units);
        combatView.SetVisible(true);
        combatView.StartCombatView(hits);
    }

    [Server]
    public Hit[] SimulateCombat(List<CombatUnit> units)
    {
        List<Hit> hits = new List<Hit>();

        units.ToList()
            .ForEach(u =>
            {
                u.PrepareForBattle(u.GetComponent<Character>().utilityStats);
                hits.Add(new Hit() { time = 0, attacker = u, attackerStats = u.GetStatsSnapshot() });
            });

        int combatRound = 1;

        do
        {
            units
                .Where(u => u.IsAlive())
                .ToList()
                .ForEach(u =>
                {
                    CombatUnit target =
                        units
                        .NotEqual(u)
                        .Where(t => t.IsAlive())
                        .GetRandomOrDefault();

                    if (target == default)
                        return;

                    if (u.RollHit(target))
                    {
                        Hit hit = u.InflictDamage(target);
                        hit.time = combatRound;
                        hits.Add(hit);
                    }

                    if (!target.IsAlive())
                    {
                        MessageBroker.Default.Publish<OnLostFight>(new OnLostFight() { loser = target });
                    }
                });

            combatRound++;
        }
        while
            (1 < units.Count(u => u.IsAlive()) && combatRound < 50);

        if (combatRound >= 50)
            throw new System.Exception("CombatRound >= 50!!!");

        return hits.ToArray();
    }


}

public struct OnLostFight
{
    public CombatUnit loser;
}

public struct Hit
{
    public float time;
    public CombatUnit attacker, defendant;
    public CombatUnit.Stats attackerStats, defendantStats;

    public override string ToString()
    {
        string res = "";
        res += "t: " + time.ToString();
        res += " | attacker: " + attacker.netId.ToString();
        // res += defendantStats.ToString();
        // res += defendant.ToString();
        // res += defendantStats.ToString();
        res += "\n";

        return res;
    }

}
