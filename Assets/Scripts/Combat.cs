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
            .Subscribe(AddCombatToList);

        FindObjectOfType<TurnSystem>()
            .onRoundEnd += StartAllCombats;
    }

    [Server]
    private void AddCombatToList(CombatUnit[] units)
    {
        if (combatList.Any(c => units.All(u => c.Contains(u))))
            return;
       
        combatList.Add(units);

        var attackerConn = units[0].netIdentity.connectionToClient;
        TargetSpawnAttackMarker(attackerConn, units);
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
    public void StartCombat(params CombatUnit[] units)
    {
        var hits = SimulateCombat(units);

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
    public Hit[] SimulateCombat(params CombatUnit[] units)
    {
        List<Hit> hits = new List<Hit>();

        units.ToList()
            .ForEach(u =>
            {
                hits.Add(new Hit() { time = 0, attacker = u, attackerStats = u.GetStatsSnapshot() });
            });

        units.ToList()
            .ForEach(u =>
            {
                float attacksPerBattle =
                    u.attackTimerRatio + combatDurationInSeconds / u.GetAttackIntervalInSeconds();

                int fullAttacks = (int)MathF.Floor(attacksPerBattle);

                fullAttacks.ForLoop(i =>
                {
                    float time = (i + 1) * u.GetAttackIntervalInSeconds() - u.attackTimerRatio;

                    hits.Add(new Hit() { time = time, attacker = u });
                });

                u.attackTimerRatio = attacksPerBattle - fullAttacks;
            });

        hits =
            hits
            .OrderBy(h => h.time)
            .ToList();

        var hitArray =
            hits
            .Select(hit =>
            {
                CombatUnit attacker = hit.attacker;

                if (attacker.health <= 0)
                    return new Hit() { time = -1 };

                CombatUnit target =
                    units
                    .NotEqual(attacker)
                    .Where(t => t.health > 0)
                    .GetRandomOrDefault();

                if (target == default)
                    return new Hit() { time = -1 };

                int damage = (int)MathF.Max(1, attacker.attack / target.defense);

                target.health -= damage;

                Hit updHit = new Hit()
                {
                    time = hit.time,
                    attacker = hit.attacker,
                    attackerStats = attacker.GetStatsSnapshot(),
                    defendant = target,
                    defendantStats = target.GetStatsSnapshot()
                };

                return updHit;
            })
            .Where(hit => hit.time >= 0)
            .ToArray();

        return hitArray;
    }


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
