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
    [SerializeField] private Image attackProgressShadow;
    [SerializeField] private Image attackProgress;

    CompositeDisposable disposables;

    public void Initialize(CombatUnit unit, IDisposable combatOngoingDisposable)
    {
        disposables = new CompositeDisposable();

        unit.healthReactive
            .Subscribe(val =>{
                healthText.text = val.ToString();
            })
            .AddTo(disposables);


        unit.attackTimerRatioReactive
            .Subscribe(val =>{
                attackProgressShadow.fillAmount = val;
                attackProgress.fillAmount = val;
            })
            .AddTo(disposables);

        combatOngoingDisposable
            .AddTo(disposables);
    }

    public void OnDisable()
    {
        disposables.Dispose();
    }
}
