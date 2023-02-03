using UnityEngine;
using System.Linq;
using Mirror;
using System;
using System.Collections.Generic;

public class TurnSystem : NetworkBehaviour
{
    [SyncVar] int currentPlayerId;
    readonly List<Player> players = new List<Player>();

    readonly Dictionary<Player, List<TurnAction>> playersActions = new Dictionary<Player, List<TurnAction>>();

    [SyncVar] Player currentPlayer;

    public void AddPlayer(Player player, params TurnAction[] turnActions)
    {
        players.Add(player);
        playersActions.Add(player, new List<TurnAction>(turnActions));
    }

    public void CheckActionsLeft()
    {
        var uses = playersActions[currentPlayer]
            .Sum(act => act.usesLeft);

        if (uses == 0)
        {
            GiveTurnToNextPlayer();
        }
    }

    private void GiveTurnToNextPlayer()
    {
        currentPlayerId++;
        currentPlayerId %= players.Count;

        currentPlayer = players[currentPlayerId];

        playersActions[currentPlayer]
            .ToList()
            .ForEach(act => act.RestoreUses());
    }

    abstract public class TurnAction
    {
        public event Action onExecuted;
        public event Action onUsesOver;
        public int uses {get; protected set;}
        public int usesLeft;

        public TurnAction(int uses)
        {
            this.uses = uses;
            this.usesLeft = uses;
        }

        protected bool UsesLeft() => usesLeft > 0;

        public void SignalExecuted()
        {
            onExecuted?.Invoke();

            if (--usesLeft <= 0)
                onUsesOver?.Invoke();
        }

        internal void RestoreUses()
        {
            usesLeft = uses;
        }
    }

    public class MoveCharacter_TurnAction : TurnAction
    {
        Character character;

        public MoveCharacter_TurnAction(Character character, int stepCount) : base(stepCount)
        {
            this.character = character;
        }

        public void Move(Vector3Int coordinates)
        {
            if (!UsesLeft())
                return;

            if (character is null ||
                coordinates == character.coordinates ||
                character.OutOfReach(coordinates)
            )
                return;

            character.Move(coordinates);

            SignalExecuted();
        }
    }
}
