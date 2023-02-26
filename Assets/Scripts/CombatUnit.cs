using System;
using Mirror;
using UniRx;

public class CombatUnit : NetworkBehaviour
{
    [SyncVar]
    private Stats
        characterStats,
        equipmentStats,
        totalStats;


    public void SetCharacterStats(Stats characterStats)
    {
        this.characterStats = characterStats;
        RecalcTotalStats();
    }
    public void SetEquipmentStats(Stats equipmentStats)
    {
        this.equipmentStats = equipmentStats;
        RecalcTotalStats();
    }

    private void RecalcTotalStats()
    {
        totalStats = characterStats + equipmentStats;
    }

    public Stats GetCharacterStats() => characterStats;
    public Stats GetEquipmentStats() => equipmentStats;
    public Stats GetTotalStats() => totalStats;

    public void PrepareForBattle(Character.UtilityStats utilityStats)
    {
        RecalcTotalStats();
        totalStats.health = 100;
    }

    [System.Serializable]
    public struct Stats
    {
        public int
            health,
            attack,
            defense,
            precision,
            agility;

        [UnityEngine.HideInInspector]
        public int
            speed;

        [UnityEngine.HideInInspector]
        public float
            attackTimerRatio;

        static public Stats operator+(Stats a, Stats b)
        {
            return new Stats()
            {
                attack = a.attack + b.attack,
                defense = a.defense + b.defense,
                precision = a.precision + b.precision,
                agility = a.agility + b.agility,
            };
        }

        public override string ToString()
        {
            return
                ("Attack: " + attack + "\n").PadLeft(20, ' ') +
                ("Defense: " + defense + "\n").PadLeft(20, ' ') +
                ("Precision: " + precision + "\n").PadLeft(20, ' ') +
                ("Agility: " + agility + "\n").PadLeft(20, ' ');
        }
    }

    public ReactiveProperty<int>
        healthReactive = new ReactiveProperty<int>();

    public ReactiveProperty<float>
        attackTimerRatioReactive = new ReactiveProperty<float>();

    [SyncVar(hook = nameof(OnHealthSync))]
    public int health;

    [SyncVar(hook = nameof(OnAttackTimerRatioSync))]
    public float attackTimerRatio;

    private void Awake()
    {
        healthReactive.Value = health;
        attackTimerRatioReactive.Value = attackTimerRatio;
        totalStats.speed = 100;
    }

    public Stats GetStatsSnapshot()
    {
        return totalStats;
    }

    public float GetAttackIntervalInSeconds() => totalStats.speed / 100f;

    public bool AttackTimerTick(float delta)
    {
        attackTimerRatio += totalStats.speed / 100f * delta;

        bool res = attackTimerRatio >= 1f;

        if (res)
            attackTimerRatio -= 1f;

        return res;
    }

    public bool FakeAttackTimerTick(ref float timer, float delta)
    {
        timer += totalStats.speed / 100f * delta;

        bool res = timer >= 1f;

        if (res)
            timer -= 1f;

        return res;
    }

    private void OnAttackTimerRatioSync(float oldR, float newR)
    {
        attackTimerRatioReactive.Value = newR;
    }

    private void OnHealthSync(int oldH, int newH)
    {
        healthReactive.Value = newH;
    }


    public bool RollHit(CombatUnit target)
    {
        int
            targetDodgeRoll = UnityEngine.Random.Range(0, target.totalStats.agility),
            thisHitRoll = UnityEngine.Random.Range(0, totalStats.precision);
      
        return thisHitRoll > targetDodgeRoll;
    }

    public Hit InflictDamage(CombatUnit target)
    {
        int damage = totalStats.attack / target.totalStats.defense;

        target.totalStats.health = Math.Max(0, target.totalStats.health - damage);

        return new Hit()
        {
            attacker = this,
            attackerStats = totalStats,
            defendant = target,
            defendantStats = target.totalStats
        };
    }

    public bool IsAlive() => GetTotalStats().health > 0;
}
