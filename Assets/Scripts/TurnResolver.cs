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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!combat.isOngoing)
            {
                CmdStartMockupCombat();
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdStartMockupCombat()
    {
        var units =
            FindObjectsOfType<Character>()
            .Select(ch => ch.combatUnit)
            .ToArray();

        if (units.Length < 2)
        {
            Debug.Log("Not enough players to start combat");
            return;
        }

        CmdStartCombat(units);
    }


    [Command(requiresAuthority = false)]
    public void CmdStartCombat(params CombatUnit[] units)
    {
        combat.CmdStartCombat(units);
    }
}
