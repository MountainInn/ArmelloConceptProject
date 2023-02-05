using UniRx;
using System;
using MountainInn;
using UnityEngine;

public class Turn
{
    CompositeDisposable
        disposables = new CompositeDisposable();

    ReactiveProperty<bool>
        explorationPhaseComplete = new ReactiveProperty<bool>(false),
        movementPhaseComplete = new ReactiveProperty<bool>(false),
        combatPhaseComplete = new ReactiveProperty<bool>(false);

    public IObservable<bool>
        started;
    public IReadOnlyReactiveProperty<bool>
        completed;

    public Turn(uint playerNetId, IObservable<uint> playerNetIdObservable)
    {
        completed =
            Observable
            .CombineLatest(explorationPhaseComplete,
                           movementPhaseComplete,
                           combatPhaseComplete,
                           BoolExt.All)
            .ToReactiveProperty();

        completed
            .Where(b => b == true)
            .Subscribe(_ =>
            {
                Debug.Log($"Player {playerNetId} Turn END");
                ResetPhases();
            })
            .AddTo(disposables);


        started =
            playerNetIdObservable
            .Select(id =>
            {
                return id == playerNetId;
            });

        started
            .Where(b => b == true)
            .Subscribe(_ =>
            {
                Debug.Log($"Player {playerNetId} Turn START");
            })
            .AddTo(disposables);

    }

    private void ResetPhases()
    {
        explorationPhaseComplete.Value = false;
        movementPhaseComplete.Value = false;
        combatPhaseComplete.Value = false;
    }

    ~Turn()
    {
        disposables.Dispose();
    }

    public void CompleteExplorationPhase()
    {
        explorationPhaseComplete.Value = true;
    }
    public void CompleteMovementPhase()
    {
        movementPhaseComplete.Value = true;
    }
    public void CompleteCombatPhase()
    {
        combatPhaseComplete.Value = true;
    }
}
