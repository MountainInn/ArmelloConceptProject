using Mirror;
using Zenject;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using System.Linq;

public class TurnResolver : NetworkBehaviour
{
    CubeMap cubeMap;
    Combat combat;

    [Inject]
    public void Construct(CubeMap cubeMap, Combat combat)
    {
        this.cubeMap = cubeMap;
        this.combat = combat;
    }

    private void Awake()
    {
        this.UpdateAsObservable()
            .Where(_ => !combat.isOngoing && Input.GetKeyDown(KeyCode.F))
            .Subscribe(_ => CmdStartMockupCombat())
            .AddTo(this);
    }

    [Server]
    [Command]
    public void CmdStartMockupCombat()
    {
        var units =
            GetComponents<Character>()
            .Select(ch => ch.combatUnit)
            .ToArray();

        if (units.Length < 2)
        {
            Debug.Log("Not enough players to start combat");
            return;
        }

        CmdStartCombat(units);
    }


    [Server]
    [Command]
    public void CmdStartCombat(params CombatUnit[] units)
    {
        combat.CmdStartCombat(units);
    }
}
