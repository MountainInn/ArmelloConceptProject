using Mirror;
using UniRx;

public class CombatUnit : NetworkBehaviour
{
    public struct NewStats
    {
        public int
            attack,
            defense,
            precision,
            agility;

        static public NewStats operator+(NewStats a, NewStats b)
        {
            return new NewStats()
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
                ("Attack: " + attack + "\n").PadLeft(18) +
                ("Defense: " + defense + "\n").PadLeft(18) +
                ("Precision: " + precision + "\n").PadLeft(18) +
                ("Agility: " + agility + "\n").PadLeft(18);
        }

    }
    public struct Stats
    {
        public int
            health,
            defense,
            attack,
            speed;
        public float
            attackTimerRatio;
    }

    public ReactiveProperty<int>
        healthReactive = new ReactiveProperty<int>();

    public ReactiveProperty<float>
        attackTimerRatioReactive = new ReactiveProperty<float>();

    [SyncVar(hook = nameof(OnHealthSync))]
    public int health;

    [SyncVar(hook = nameof(OnAttackTimerRatioSync))]
    public float attackTimerRatio;

    [SyncVar]
    public int
        defense,
        attack,
        speed;

    private void Awake()
    {
        healthReactive.Value = health;
        attackTimerRatioReactive.Value = attackTimerRatio;
    }

    public Stats GetStatsSnapshot()
    {
        return new Stats()
        {
            health = health,
                defense = defense,
                attack = attack,
                speed = speed
                };
    }

    public float GetAttackIntervalInSeconds() => speed / 100f;

    public bool AttackTimerTick(float delta)
    {
        attackTimerRatio += speed / 100f * delta;

        bool res = attackTimerRatio >= 1f;

        if (res)
            attackTimerRatio -= 1f;

        return res;
    }

    public bool FakeAttackTimerTick(ref float timer, float delta)
    {
        timer += speed / 100f * delta;

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

}
