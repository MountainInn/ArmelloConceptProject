using UnityEngine;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using Zenject;
using MountainInn;
using System.Collections.Generic;
using System;

public class CombatView : MonoBehaviour
{
    [SerializeField]
    Combat combat;

    RectTransform rect;
    CanvasGroup canvasGroup;

    StatView statViewPrefab;
    Dictionary<CombatUnit, StatView> unitViews = new Dictionary<CombatUnit, StatView>();

    IDisposable combatDisposable;

    [Inject]
    public void Construct(StatView prefab)
    {
        statViewPrefab = prefab;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        canvasGroup.alpha = (visible) ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void InitCombatView(CombatUnit[] units)
    {
        float angleInterval = Mathf.PI * 2 / units.Length;

        units
            .Enumerate()
            .ToList()
            .ForEach(tup =>
            {
                (int i, CombatUnit unit) = tup;

                float a = i * angleInterval;
                Vector2 localPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                localPosition.Scale(rect.rect.size * 0.75f * 0.5f);

                var statView = GetStatView(unit);

                statView.InitUnit(unit);
                statView.transform.SetParent(transform);
                statView.transform.localPosition = localPosition;
            });
    }

    public void StartCombatView(Hit[] hits)
    {
        combatDisposable?.Dispose();
        combatDisposable = null;

        var hitQueue = new Queue<Hit>(hits);

        var nextHit = hitQueue.Peek();
        float battleTimer = 0f;

        combatDisposable =
            this.UpdateAsObservable()
            .TakeWhile((_) => hitQueue.Any())
            .DoOnCompleted(() => SetVisible(false))
            .Subscribe((_) =>
            {
                float delta = Time.deltaTime;

                battleTimer += delta;

                unitViews.Values
                    .ToList()
                    .ForEach(view => view.TickAttackProgress(delta));

                while (battleTimer >= nextHit.time)
                {
                    Hit hit = hitQueue.Dequeue();

                    StatView
                        attackerView = unitViews[hit.attacker],
                        defendantView = unitViews[hit.defendant];

                    attackerView.SetStats(hit.attackerStats);
                    defendantView.SetStats(hit.defendantStats);

                    if (!hitQueue.Any())
                        break;

                    nextHit = hitQueue.Peek();
                }
            });
    }

    private StatView GetStatView(CombatUnit unit)
    {
        Debug.Log($"GetStatView");
        if (unitViews.ContainsKey(unit))
            return unitViews[unit];

        Debug.Log($"+Init New");
        StatView newUnitView = Instantiate(statViewPrefab);

        unitViews.Add(unit, newUnitView);

        return newUnitView;
    }

}

