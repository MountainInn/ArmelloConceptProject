using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System;
using UniRx;
using UniRx.Triggers;
using MountainInn;

public class Combat : NetworkBehaviour
{
    IDisposable
        battleSubscription;

    public ReactiveProperty<bool>
        isOngoing = new ReactiveProperty<bool>(false);

    public void StartCombat(params Stats[] units)
    {
        battleSubscription =
            this
            .UpdateAsObservable()
            .SelectMany(_ => units)
            .Subscribe(u =>
                       u.SimulateBattle(units, Time.deltaTime));

        isOngoing.Value = true;
    }

    public void EndCombat()
    {
        battleSubscription?.Dispose();

        isOngoing.Value = false;
    }

    public class Stats
    {
        public ReactiveProperty<int>
            health = new ReactiveProperty<int>(0);

        public ReactiveProperty<float>
            attackTimerRatio = new ReactiveProperty<float>(0);

        public int
            defense,
            attack,
            speed;

        public void SimulateBattle(Stats[] allUnits, float delta)
        {
            if (AttackTimerTick(delta))
            {
                Stats target =
                    allUnits
                    .NotEquals(this)
                    .GetRandom();

                MakeAttack(target);
            }
        }
        public bool AttackTimerTick(float delta)
        {
            attackTimerRatio.Value += speed / 100f * delta;

            bool res = attackTimerRatio.Value >= 1f;

            if (res)
                attackTimerRatio.Value -= 1f;

            return res;
        }

        public void MakeAttack(Stats other)
        {
            int damageTaken = Mathf.Max(1, attack / other.defense);

            other.health.Value -= damageTaken;
        }
    }
}
