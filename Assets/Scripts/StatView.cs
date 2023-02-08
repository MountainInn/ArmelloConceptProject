using UniRx;
using MountainInn;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StatView : MonoBehaviour
{
    private Text playerName;
    private Text healthText;
    private Image attackProgress;

    CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(Combat.CombatUnit unit, IDisposable combatOngoingDisposable)
    {
        unit.health
            .Subscribe(val =>{
                healthText.text = val.ToString();
            })
            .AddTo(disposables);

        unit.attackTimerRatio
            .Subscribe(val =>{
                attackProgress.fillAmount = val;
            })
            .AddTo(disposables);

        combatOngoingDisposable
            .AddTo(disposables);
    }

    private void OnDisable()
    {
        disposables.Dispose();
    }
}

