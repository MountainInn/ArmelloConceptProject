using UniRx;
using UniRx.Triggers;
using MountainInn;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using System.Collections.Generic;

public class StatView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image attackProgressShadow;
    [SerializeField] private Image attackProgress;

    CompositeDisposable disposables;

    public void Initialize(HitLog hitlog, IDisposable combatOngoingDisposable)
    {
        disposables = new CompositeDisposable();

        CombatUnit unit = hitlog.unit;

        Queue<Hit> hits =
            new Queue<Hit>(hitlog.hits.ToList());


        this.UpdateAsObservable()
            .Where(_ =>
                   hits.Any() &&
                   unit.AttackTimerTick(Time.deltaTime))
            .Subscribe(_ =>
            {
                Hit hit = hits.Dequeue();


            });

        // attackStream
        //     .Where(_ => !hits.Any())
        //     .Subscribe(_ => )



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
