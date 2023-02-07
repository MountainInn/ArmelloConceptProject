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
            .Where(_ => !combat.isOngoing.Value && Input.GetKeyDown(KeyCode.F))
            .Subscribe(_ =>
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

                StartCombat(units);
            })
            .AddTo(this);
    }

    [Server]
    public void StartCombat(params Combat.CombatUnit[] units)
    {
        combat.SrvStartCombat(units);
    }
}
