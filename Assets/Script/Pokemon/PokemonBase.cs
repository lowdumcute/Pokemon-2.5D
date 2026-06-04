using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/New Pokemon")]
public class PokemonBase : ScriptableObject
{
    [Header("Base Info")]
    [SerializeField] string pokeName;

    [TextArea] [SerializeField] private string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite iconSprite;
    [SerializeField] public PokemonType type1;
    [SerializeField] public PokemonType type2;
    [Header("Stats")]
    [SerializeField] private int maxHP;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int spAttack;
    [SerializeField] private int spDefense;
    [SerializeField] private int speed;
    [Header("Learnable Moves")]
    [SerializeField] List<learnableMove> learnableMoves;
    [Header("Experience Growth")]
    [SerializeField] int expYield;
    public int ExpYield => expYield;
    [SerializeField] GrowthRate growthRate;
    public GrowthRate GrowthRate => growthRate;
    [SerializeField] private Evolution evolution;
    public Evolution Evolution => evolution;
    [Header("Mega Evolution")]
    public MegaEvolution MegaEvolution;

    [Header("Catch Rate")]
    private int catchRate = 255;
    public int CatchRate => catchRate;

    

    public string Name
    {
        get { return pokeName; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public Sprite IconSprite
    {
        get { return iconSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public int MaxHP
    {
        get { return maxHP; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<learnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}
[System.Serializable]
public class learnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}
[System.Serializable]
public class MegaEvolution
{
    public PokemonBase megaForm;
    public string requiredItem; // ví dụ: "Charizardite X"
}
[System.Serializable]
public class Evolution
{
    public PokemonBase evolvedForm; // Pokémon sau khi tiến hóa
    public int requiredLevel;       // Level cần thiết để tiến hóa
}
public enum EvolutionType
{
    LevelUp, // Tiến hóa khi đạt đến level nhất định
    UseItem, // Tiến hóa khi sử dụng một vật phẩm
    Trade,   // Tiến hóa khi trao đổi với người chơi khác
    None     // Không có tiến hóa
}
public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Fairy,
    Dark,
    Steel,
}
public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}
public enum GrowthRate
{
    Fast,
    MediumFast,
    MediumSlow,
    Slow
}
public class TypeChart
{
    static float[][] chart =
    {
        //                        NOR  FIR  WAT  ELE  GRA  ICE  FIG  POI  GRO  FLY  PSY  BUG  ROC  GHO  DRA  DAR  STE  FAI
        /*Normal*/   new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f,   1f, 1f, 0.5f, 1f},
        /*Fire*/     new float[] {1f, 0.5f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f, 2f, 1f},
        /*Water*/    new float[] {1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f, 1f},
        /*Grass*/    new float[] {1f, 0.5f, 2f, 0.5f, 0.5f, 1f, 1f, 0.5f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 0.5f, 1f},
        /*Electric*/ new float[] {1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f},
        /*Ice*/      new float[] {1f, 0.5f, 0.5f, 2f, 1f, 0.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f},
        /*Fighting*/ new float[] {2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0f, 1f, 2f, 2f, 0.5f},
        /*Poison*/   new float[] {1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 0f, 2f},
        /*Ground*/   new float[] {1f, 2f, 1f, 0.5f, 2f, 1f, 1f, 2f, 1f, 0f, 1f, 0.5f, 2f, 1f, 1f, 1f, 2f, 1f},
        /*Flying*/   new float[] {1f, 1f, 1f, 2f, 0.5f, 1f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 1f},
        /*Psychic*/  new float[] {1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 0f, 0.5f, 1f},
        /*Bug*/      new float[] {1f, 0.5f, 1f, 0.5f, 1f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 0.5f, 1f, 2f, 0.5f, 0.5f},
        /*Rock*/     new float[] {1f, 2f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /*Ghost*/    new float[] {0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f},
        /*Dragon*/   new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 0f},
        /*Dark*/     new float[] {1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f},
        /*Steel*/    new float[] {1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 2f},
        /*Fairy*/    new float[] {1f, 0.5f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 0.5f, 1f}
    };
    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        int row = (int)attackType - 1;
        int column = (int)defenseType - 1;
        return chart[row][column];
    }
}

