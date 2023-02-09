using Mirror;
using Zenject;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using System.Linq;

public class TurnResolver : NetworkBehaviour
{
    Combat combat;
    CubeMap cubeMap;

    private void Start()
    {
        cubeMap = FindObjectOfType<CubeMap>();
        combat = FindObjectOfType<Combat>();
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
