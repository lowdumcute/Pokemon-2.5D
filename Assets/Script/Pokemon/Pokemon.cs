
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] gender gender;
    public PokemonBase Base { get { return _base; } }
    public int Level { get { return level; } }
    public int CurrentHp { get; set; }
    public List<Move> Moves { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    // Trạng thái hiện tại của Pokémon
    public Condition Status { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    // VolatileStatus

    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public ConditionID StatusID { get; private set; } = ConditionID.none;
    public int StatusTime { get; set; } // Đếm lượt (cho sleep)
    public bool HpChanged { get; set; }
    public bool ExpChanged { get; set; }
    public int currentExp { get; private set; }
    public int ExpToNextLevel => GetExpForLevel(Level + 1) - currentExp;
    public bool IsMegaEvolved { get; private set; } = false;
    public ItemBase HeldItem;
    private PokemonBase originalBase;
    public Dictionary<Stat, int> StatModifiers { get; private set; }
    public bool HurtItself { get; set; } = false;
    public void Init() // lieen quan
    {
        // Chắc chắn khởi tạo ở đây
        StatusChanges = new Queue<string>();

        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= 4)
            {
                break;
            }
        }

        caculateStats();

        CurrentHp = MaxHP;
        resetStatBoosts();
        StatModifiers = new Dictionary<Stat, int>
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 }
        };
    }

    void caculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);
        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10;
    }
    void resetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
        };
    }
    int GetStats(Stat stat)
    {
        int StatVal = Stats[stat];
        // TODO: áp dụng tăng chỉ số 
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };
        if (boost >= 0)
        {
            StatVal = Mathf.FloorToInt(StatVal * boostValues[boost]);
        }
        else
        {
            StatVal = Mathf.FloorToInt(StatVal / boostValues[-boost]);
        }

        return StatVal;
    }

    public void ApplyBoosts(List<MoveBase.StatBoost> statboosts)
    {
        foreach (var statBoost in statboosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
            Debug.Log($"{stat} đã tăng {StatBoosts[stat]}");
        }
    }
    public Pokemon(PokemonBase pBase, int level, int Hp) // lieen quan 
    {
        this._base = pBase;
        this.level = level;


        Init();
        this.CurrentHp = Mathf.Clamp(Hp, 1, MaxHP);
    }
    public void Init(int? overrideHp = null)
    {
        StatusChanges = new Queue<string>();

        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
            if (Moves.Count >= 4)
                break;
        }

        caculateStats();
        resetStatBoosts();

        if (overrideHp != null)
            CurrentHp = Mathf.Clamp(overrideHp.Value, 1, MaxHP);
        else
            CurrentHp = MaxHP;
    }
    // Mega tiến hóa, trả về true nếu thành công
    public bool TryMegaEvolve(string heldItem)
    {
        if (IsMegaEvolved) return false;
        if (Base.MegaEvolution == null) return false;
        if (Base.MegaEvolution.requiredItem != heldItem) return false;

        Debug.Log($"{Base.Name} is mega evolving!");

        originalBase = _base;

        // 🔒 Lưu lại các chiêu cũ
        var oldMoves = new List<Move>(Moves);

        // 🔁 Tiến hóa Mega
        _base = Base.MegaEvolution.megaForm;
        Init(); // cập nhật chỉ số

        // ✅ Khôi phục chiêu cũ
        Moves = oldMoves;

        IsMegaEvolved = true;
        return true;
    }
    public void RevertMegaForm()
    {
        if (IsMegaEvolved && originalBase != null)
        {
            Debug.Log($"{Base.Name} reverted to normal form.");
            _base = originalBase;
            Init();
            IsMegaEvolved = false;
            originalBase = null;
        }
    }
    public int Attack
    {
        get { return GetStats(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStats(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStats(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStats(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStats(Stat.Speed); }
    }
    public int MaxHP { get; private set; }
    public bool GainExp(int amount)
    {
        bool leveledUp = false;
        currentExp += amount;
        ExpChanged = true;

        Debug.Log($"[GainExp] Gained {amount} EXP. Current EXP: {currentExp}. Current Level: {level}");

        int nextLevelExp = GetExpForLevel(level + 1);
        Debug.Log($"[EXP Check] Need {nextLevelExp} EXP for Level {level + 1}.");

        // Kiểm tra lên cấp
        while (currentExp >= GetExpForLevel(level + 1))
        {
            currentExp -= GetExpForLevel(level + 1);
            level++;
            leveledUp = true;

            Debug.Log($"Leveled up to {level}");

            caculateStats();
            CurrentHp = MaxHP;

            // TODO: học chiêu mới nếu có

            // Tiến hóa nếu đủ điều kiện
            if (CanEvolve())
            {
                var evolvedPokemon = Evolve();
                _base = evolvedPokemon.Base;
                Init(); // cập nhật lại chỉ số, chiêu v.v. sau khi tiến hóa
            }
        }

        return leveledUp;
    }
    public int GetExpForLevel(int level)
    {
        switch (_base.GrowthRate)
        {
            case GrowthRate.Fast:
                return Mathf.FloorToInt(0.8f * Mathf.Pow(level, 3));
            case GrowthRate.MediumFast:
                return Mathf.FloorToInt(Mathf.Pow(level, 3));
            case GrowthRate.MediumSlow:
                return Mathf.FloorToInt(1.2f * Mathf.Pow(level, 3) - 15 * Mathf.Pow(level, 2) + 100 * level - 140);
            case GrowthRate.Slow:
                return Mathf.FloorToInt(1.25f * Mathf.Pow(level, 3));
            default:
                return Mathf.FloorToInt(Mathf.Pow(level, 3)); // fallback
        }
    }
    public bool CanEvolve()
    {
        return Base.Evolution != null &&
            Base.Evolution.evolvedForm != null &&
            Level >= Base.Evolution.requiredLevel;
    }

    public Pokemon Evolve()
    {
        if (!CanEvolve())
            return this;

        Debug.Log($"{Base.Name} is evolving to {Base.Evolution.evolvedForm.Name}!");

        GameManager.Instance.evolutionScreen.SetData(this);
        GameManager.Instance.evolutionScreen.gameObject.SetActive(true);

        return new Pokemon(Base.Evolution.evolvedForm, Level, CurrentHp); // giữ nguyên level, HP
    }
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var DamageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveBase.MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveBase.MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);
        return DamageDetails;
    }
    public void UpdateHP(int damage)
    {
        CurrentHp = Mathf.Clamp(CurrentHp - damage, 0, MaxHP);
        HpChanged = true;
    }
    public void setStatus(ConditionID conditionID)
    {
        // Đã có status → không đặt lại
        if (Status != null) return;

        // 🔥 Miễn nhiễm Burn nếu là hệ Fire
        if (conditionID == ConditionID.brn &&
            (Base.Type1 == PokemonType.Fire || Base.Type2 == PokemonType.Fire))
        {
            Debug.Log($"{Base.Name} is Fire-type and immune to burn!");
            return;
        }

        // 🟢🟣 Miễn nhiễm Poison nếu là Poison hoặc Steel
        if (conditionID == ConditionID.psn &&
            (Base.Type1 == PokemonType.Poison || Base.Type2 == PokemonType.Poison ||
            Base.Type1 == PokemonType.Steel || Base.Type2 == PokemonType.Steel))
        {
            Debug.Log($"{Base.Name} is Poison/Steel-type and immune to poison!");
            return;
        }

        // ❄ Miễn nhiễm Freeze nếu là Ice-type
        if (conditionID == ConditionID.frz &&
            (Base.Type1 == PokemonType.Ice || Base.Type2 == PokemonType.Ice))
        {
            Debug.Log($"{Base.Name} is Ice-type and immune to freeze!");
            return;
        }

        // Nếu không miễn → áp dụng trạng thái
        Status = ConditionDB.Conditions[conditionID];
        StatusID = conditionID;
        // ✅ GỌI OnStart nếu có
        Status?.OnStart?.Invoke(this);

        // Thêm message
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }
    public void CureStatus()
    {

        Status = null;
        StatusID = ConditionID.none;
        StatusTime = 0;
    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
    }
    public void OnBattleOver()
    {
        resetStatBoosts();
    }
    public void IncreaseHP(int amount)
    {
        CurrentHp += amount;
    }
    public void ApplyMoveEffects(Move move)
    {
        var effects = move.Base.Effects;
        if (effects == null)
            return;

        // Áp dụng tăng/giảm chỉ số (Stat Boosts)
        if (effects.Boosts != null && effects.Boosts.Count > 0)
            ApplyBoosts(effects.Boosts);

        // Áp dụng trạng thái (Status Effect)
        if (Status == null && effects.Status != ConditionID.none)
        {
            setStatus(effects.Status);
        }
    }
    public void SetVolatileStatus(VolatileConditionID id)
    {
        if (!VolatileConditionDB.Conditions.ContainsKey(id))
        {
            Debug.LogError($"VolatileStatus {id} chưa được khai báo trong VolatileConditionDB!");
            return;
        }

        VolatileStatus = VolatileConditionDB.Conditions[id];
        VolatileStatus?.OnStart?.Invoke(this);
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
        VolatileStatusTime = 0;
    }
}
public enum gender
{
    male, female
}
public class DamageDetails
{
    public float TypeEffectiveness { get; set; }
    public float Critical { get; set; }
    public bool Fainted { get; set; }
}
