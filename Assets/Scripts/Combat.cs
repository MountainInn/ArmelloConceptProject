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
        HitLog[] hitLogs = CmdSimulateCombat(units);

        units
            .Select(u => u.netIdentity.connectionToClient)
            .ToList()
            .ForEach(conn => TargetInitCombatViews(conn, hitLogs));
    }

    [TargetRpc]
    public void TargetInitCombatViews(NetworkConnection target, params HitLog[] hitlogs)
    {
        combatView.InitStatsView(units);
    }

    [Command(requiresAuthority=false)]
    public HitLog[] CmdSimulateCombat(params CombatUnit[] units)
    {
        HitLog[] hitlogs =
            units.Select(u =>
            {
                float attacksPerBattle =
                    u.stats.attackTimerRatio + combatDurationInSeconds * (u.stats.speed / 100f);
                int  fullAttacks = (int)MathF.Floor(attacksPerBattle);

                u.stats.attackTimerRatio = attacksPerBattle - fullAttacks;

                Hit[] hits =
                    fullAttacks.ToRange()
                    .Select(i =>
                    {
                        CombatUnit target =
                            units
                            .NotEqual(u)
                            .Where(t => t.stats.health > 0)
                            .GetRandom();

                        int damage = (int)MathF.Max(1, u.stats.attack / target.stats.defense);
                        target.stats.health -= damage;

                        return new Hit(){ target = target, targetStatsAfterHit = target.stats };
                    })
                    .ToArray();

                return new HitLog(){ unit = u, hits = hits };
            })
            .ToArray();

        return hitlogs;
    }


}


public struct HitLog
{
    public  CombatUnit unit;
    public  Hit[] hits;
}

public struct Hit
{
    public CombatUnit target;
    public CombatUnit.Stats targetStatsAfterHit;
}
