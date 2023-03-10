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

                var statView = GetStatView(unit, transform);

                statView.gameObject.SetActive(true);
                statView.InitUnit(unit);
                statView.transform.localPosition = localPosition;
                statView.transform.localEulerAngles = Vector3.zero;
            });
    }

    public void StartCombatView(Hit[] hits)
    {
        combatDisposable?.Dispose();
        combatDisposable = null;

        hits
            .Where(hit => hit.time == 0)
            .ToList()
            .ForEach(hit => unitViews[hit.attacker].SetStats(hit.attackerStats));

        hits =
            hits
            .Where(hit => hit.time > 0)
            .ToArray();

        var hitQueue = new Queue<Hit>(hits);

        var nextHit = hitQueue.Peek();
        float battleTimer = 0f;

        combatDisposable =
            this.UpdateAsObservable()
            .TakeWhile((_) => hitQueue.Any())
            .DoOnCompleted(() => this.StartInvokeAfter(() => SetVisible(false), 2f))
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

    private StatView GetStatView(CombatUnit unit, Transform parent)
    {
        if (unitViews.ContainsKey(unit))
            return unitViews[unit];

        StatView newUnitView = Instantiate(statViewPrefab, Vector3.zero, Quaternion.identity, parent);

        unitViews.Add(unit, newUnitView);

        return newUnitView;
    }

}

