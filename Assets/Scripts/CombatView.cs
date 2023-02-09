using UnityEngine;
using System.Linq;
using UniRx;
using UniRx.Toolkit;
using Zenject;

public class CombatView : MonoBehaviour
{
    [SerializeField]
    Combat combat;

    RectTransform rect;
    CanvasGroup canvasGroup;

    StatViewPool statViewPool;

    [Inject]
    public void Construct(StatView prefab)
    {
        statViewPool = new StatViewPool(prefab, GetComponent<RectTransform>());
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
    }

    public void InitStatsView(params CombatUnit[] units)
    {
        float angleInterval = Mathf.PI * 2 / units.Length;

        Enumerable
            .Range(0, units.Length)
            .ToList()
            .ForEach(i =>
            {
                float a = i * angleInterval;
                Vector2 localPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                localPosition.Scale(rect.rect.size * 0.75f * 0.5f);

                var statView = statViewPool.Rent();
                statView.transform.localPosition = localPosition;

                var combatOngoingDisposable =
                    combat.isOngoingReactive
                    .Where(b => b == false)
                    .Subscribe(_ => statViewPool.Return(statView));

                statView.Initialize(units[i], combatOngoingDisposable);
            });
    }

    public class StatViewPool : ObjectPool<StatView>
    {
        StatView prefab;
        RectTransform parent;

        public StatViewPool(StatView prefab, RectTransform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        protected override StatView CreateInstance()
        {
            var newStatView = GameObject.Instantiate(prefab, parent);

            return newStatView;
        }
    }
}

