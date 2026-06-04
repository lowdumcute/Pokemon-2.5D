using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/New Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string moveName;
    [TextArea] [SerializeField] string description;
    [SerializeField] private PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTartget target;
    public string Name
    {
        get { return moveName; }
    }
    public string Description
    {
        get { return description; }
    }
    public PokemonType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }
    public MoveCategory Category
    {
        get { return category; }
    }
    public MoveEffects Effects
    {
        get { return effects; }
    }
    public MoveTartget Target
    {
        get { return target; }
    }

    [System.Serializable]
    public class MoveEffects
    {
        [SerializeField] List<StatBoost> boosts;
        [SerializeField] ConditionID status;
        [SerializeField] int statusChance; // ← xác suất gây hiệu ứng (tính theo %)
        public VolatileConditionID VolatileStatus;      // ✅ Thêm cái này
        public int VolatileStatusChance = 100;          // ✅ Mặc định 100%
        public List<StatBoost> Boosts
        {
            get { return boosts; }
        }
        public ConditionID Status
        {
            get { return status; }
        }
        public int StatusChance => statusChance;
    }
    [System.Serializable]
    public class StatBoost
    {
        public Stat stat;
        public int boost;   
    }
    public enum MoveCategory
    {
        Physical,
        Special,
        Status
    }
    public enum MoveTartget
    {
        Foe, Self
    }
}
