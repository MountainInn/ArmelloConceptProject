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

    float attackRatio;

    public void InitUnit(CombatUnit unit)
    {
        attackRatio = unit.attackTimerRatio;
    }

    public void SetStats(CombatUnit.Stats stats)
    {
        healthText.text = stats.health.ToString();
        attackRatio = stats.attackTimerRatio;

        UpdateAttackTimer();

    }

    public void TickAttackProgress(float delta)
    {
        attackRatio += delta;

        if (attackRatio >= 1f)
            attackRatio -= 1f;

        UpdateAttackTimer();
    }

    private void UpdateAttackTimer()
    {
        attackProgressShadow.fillAmount = attackRatio;
        attackProgress.fillAmount = attackRatio;
    }
}
