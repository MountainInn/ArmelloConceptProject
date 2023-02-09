using UniRx;
using MountainInn;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class StatView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image attackProgress;

    CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(CombatUnit unit, IDisposable combatOngoingDisposable)
    {
        unit.healthReactive
            .Subscribe(val =>{
                healthText.text = val.ToString();
            })
            .AddTo(disposables);

        unit.attackTimerRatioReactive
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

