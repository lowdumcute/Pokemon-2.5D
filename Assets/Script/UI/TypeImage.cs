using UnityEngine;
[CreateAssetMenu(fileName = "TypeDataUI", menuName = "TypeDataUI/New TypeDataUI")]
public class TypeSprites : ScriptableObject
{
    public Sprite NormalSprite;
    public Sprite FireSprite;
    public Sprite WaterSprite;
    public Sprite ElectricSprite;
    public Sprite GrassSprite;
    public Sprite IceSprite;
    public Sprite FightingSprite;
    public Sprite PoisonSprite;
    public Sprite GroundSprite;
    public Sprite FlyingSprite;
    public Sprite PsychicSprite;
    public Sprite BugSprite;
    public Sprite RockSprite;
    public Sprite GhostSprite;
    public Sprite DragonSprite;
    public Sprite DarkSprite;
    public Sprite SteelSprite;
    public Sprite FairySprite;

    [Header("Moves Sprites")]
    public Sprite NormalMoveSprite;
    public Sprite FireMoveSprite;
    public Sprite WaterMoveSprite;
    public Sprite ElectricMoveSprite;
    public Sprite GrassMoveSprite;
    public Sprite IceMoveSprite;
    public Sprite FightingMoveSprite;
    public Sprite PoisonMoveSprite;
    public Sprite GroundMoveSprite;
    public Sprite FlyingMoveSprite;
    public Sprite PsychicMoveSprite;
    public Sprite BugMoveSprite;
    public Sprite RockMoveSprite;
    public Sprite GhostMoveSprite;
    public Sprite DragonMoveSprite;
    public Sprite DarkMoveSprite;
    public Sprite SteelMoveSprite;
    public Sprite FairyMoveSprite;
    [Header("Status Sprites")]
    public Sprite BurnStatusSprite;
    public Sprite FreezeStatusSprite;
    public Sprite ParalysisStatusSprite;
    public Sprite PoisonStatusSprite;
    public Sprite SleepStatusSprite;



    public Sprite GetTypeSprite(PokemonType type)
    {
        switch (type)
        {
            case PokemonType.Normal: return NormalSprite;
            case PokemonType.Fire: return FireSprite;
            case PokemonType.Water: return WaterSprite;
            case PokemonType.Electric: return ElectricSprite;
            case PokemonType.Grass: return GrassSprite;
            case PokemonType.Ice: return IceSprite;
            case PokemonType.Fighting: return FightingSprite;
            case PokemonType.Poison: return PoisonSprite;
            case PokemonType.Ground: return GroundSprite;
            case PokemonType.Flying: return FlyingSprite;
            case PokemonType.Psychic: return PsychicSprite;
            case PokemonType.Bug: return BugSprite;
            case PokemonType.Rock: return RockSprite;
            case PokemonType.Ghost: return GhostSprite;
            case PokemonType.Dragon: return DragonSprite;
            case PokemonType.Dark: return DarkSprite;
            case PokemonType.Steel: return SteelSprite;
            case PokemonType.Fairy: return FairySprite;
            default: return null;
        }
    }
    public Sprite GetMoveSprite(PokemonType type)
    {
        switch (type)
        {
            case PokemonType.Normal: return NormalMoveSprite;
            case PokemonType.Fire: return FireMoveSprite;
            case PokemonType.Water: return WaterMoveSprite;
            case PokemonType.Electric: return ElectricMoveSprite;
            case PokemonType.Grass: return GrassMoveSprite;
            case PokemonType.Ice: return IceMoveSprite;
            case PokemonType.Fighting: return FightingMoveSprite;
            case PokemonType.Poison: return PoisonMoveSprite;
            case PokemonType.Ground: return GroundMoveSprite;
            case PokemonType.Flying: return FlyingMoveSprite;
            case PokemonType.Psychic: return PsychicMoveSprite;
            case PokemonType.Bug: return BugMoveSprite;
            case PokemonType.Rock: return RockMoveSprite;
            case PokemonType.Ghost: return GhostMoveSprite;
            case PokemonType.Dragon: return DragonMoveSprite;
            case PokemonType.Dark: return DarkMoveSprite;
            case PokemonType.Steel: return SteelMoveSprite;
            case PokemonType.Fairy: return FairyMoveSprite;
            default: return null;
        }
    }
    public Sprite GetStatusSprite(ConditionID condition)
    {
        switch (condition)
        {
            case ConditionID.brn: return BurnStatusSprite;
            case ConditionID.frz: return FreezeStatusSprite;
            case ConditionID.par: return ParalysisStatusSprite;
            case ConditionID.psn: return PoisonStatusSprite;
            case ConditionID.slp: return SleepStatusSprite;
            default: return null;
        }
    }
}