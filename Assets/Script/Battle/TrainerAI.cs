using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum định nghĩa tính cách của trainer NPC
/// </summary>
public enum TrainerPersonality
{
    Aggressive,  // Tấn công mạnh, ít hồi máu, thích switch nếu bị ưỡng
    Tactical,    // Đọc type advantage, xét kế hoạch, cân bằng defense/offense
    Coward       // Thủ nhiều, heal khi máu thấp, switch nếu cần
}

/// Script xử lý logic chọn chiêu cho trainer battle
/// Được tách riêng khỏi BattleSystem để dễ quản lý
/// </summary>
public class TrainerAI
{
    private TrainerPersonality personality;

    public TrainerAI(TrainerPersonality personality)
    {
        this.personality = personality;
    }

    /// <summary>
    /// Hàm chính để chọn move dựa trên tính cách trainer
    /// </summary>
    public Move SelectMove(Pokemon attacker, Pokemon defender)
    {
        switch (personality)
        {
            case TrainerPersonality.Aggressive:
                return GetAggressiveMove(attacker, defender);
            case TrainerPersonality.Coward:
                return GetCowardMove(attacker, defender);
            default:
                return GetTacticalMove(attacker, defender);
        }
    }    // ============ AGGRESSIVE TRAINER ============
    // Mục tiêu: Damage lớn nhất, nhưng vẫn xét lợi thế hệ, thích switch nếu bị ưỡng
    private Move GetAggressiveMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -1f;

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0) continue;

            float score = EstimateDamagePotential(move, attacker, defender);
            float typeEff = GetTypeEffectiveness(move, defender);
            
            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 0.5f; // Aggressive rất ít dùng status
            }
            else
            {
                // Bonus nhẹ cho lợi thế hệ
                score += typeEff * 3f;
                // Nếu opponent sắp chết, priority damage
                if (defender.CurrentHp < defender.MaxHP * 0.2f)
                    score *= 1.5f;
                // Thêm random nhỏ để không dễ đoán
                score += Random.Range(-2f, 2f);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }    // ============ TACTICAL TRAINER ============
    // Mục tiêu: Cân nhắc lợi thế hệ, tiết kiệm PP, dùng status hợp lý, switch hợp lý
    private Move GetTacticalMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -999f;

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0) continue;

            float score;
            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = EvaluateStatusMove(move, attacker, defender);
            }
            else
            {
                score = EstimateDamagePotential(move, attacker, defender);
                float typeEff = GetTypeEffectiveness(move, defender);
                score += 2.5f * typeEff; // ưu tiên lợi thế hệ
                
                // Nếu opponent sắp chết, priority damage
                if (defender.CurrentHp < defender.MaxHP * 0.3f)
                    score *= 1.3f;
                
                score *= 1f - (1f / (move.PP + 1f)); // giữ tài nguyên PP
                score += Random.Range(-1f, 1f); // random nhỏ
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }    // ============ COWARD TRAINER ============
    // Mục tiêu: Máu thấp thì heal/buff, chọn chiêu an toàn, switch nếu bị khắc chếchế
    private Move GetCowardMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -999f;
        float hpRatio = (float)attacker.CurrentHp / attacker.MaxHP;

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0) continue;

            float score;
            
            // Nếu máu dưới 40%, ưu tiên heal/buff bản thân
            if (hpRatio <= 0.4f && move.Base.Target == MoveBase.MoveTartget.Self && 
                move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 150f; // ưu tiên cao nhất
            }
            // Nếu máu dưới 60%, tìm move heal/buff
            else if (hpRatio <= 0.6f && move.Base.Target == MoveBase.MoveTartget.Self && 
                     move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 80f;
            }
            else if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 15f + EvaluateStatusMove(move, attacker, defender);
            }
            else
            {
                float effectiveness = GetTypeEffectiveness(move, defender);
                // Chọn chiêu an toàn (có lợi thế)
                score = effectiveness > 1f ? 40f : 20f;
                score += effectiveness * 10f;
                
                // Nếu opponent sắp chết, cân nhắc finish
                if (defender.CurrentHp < defender.MaxHP * 0.2f)
                    score *= 1.2f;
                    
                score += Random.Range(-2f, 2f);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    // ============ HELPER FUNCTIONS ============

    private float EstimateDamagePotential(Move move, Pokemon attacker, Pokemon defender)
    {
        if (move.Base.Category == MoveBase.MoveCategory.Status)
            return 0f;

        float typeEffectiveness = GetTypeEffectiveness(move, defender);
        return move.Base.Power * typeEffectiveness;
    }

    private float GetTypeEffectiveness(Move move, Pokemon defender)
    {
        float effect1 = TypeChart.GetEffectiveness(move.Base.Type, defender.Base.Type1);
        float effect2 = TypeChart.GetEffectiveness(move.Base.Type, defender.Base.Type2);
        return effect1 * effect2;
    }    private float EvaluateStatusMove(Move move, Pokemon attacker, Pokemon defender)
    {
        float score = 0f;
        if (move.Base.Target == MoveBase.MoveTartget.Self)
        {
            // Self-buff: tăng stats
            if (move.Base.Effects != null && move.Base.Effects.Boosts != null && move.Base.Effects.Boosts.Count > 0)
            {
                score += 15f + move.Base.Effects.Boosts.Count * 3f;
            }
            // Self-heal status
            if (move.Base.Effects != null && move.Base.Effects.Status != ConditionID.none)
            {
                score += 8f;
            }
        }
        else
        {
            // Debuff opponent
            if (move.Base.Effects != null && move.Base.Effects.Status != ConditionID.none)
            {
                score += 12f;
                // Nếu opponent chưa có status, ưu tiên cao hơn
                if (defender.Status == null)
                    score += 5f;
            }
            // Volatile status (confusion, flinch...)
            if (move.Base.Effects != null && move.Base.Effects.VolatileStatus != VolatileConditionID.none)
            {
                score += 8f;
            }
        }
        return score;
    }

    /// <summary>
    /// Helper để check xem trainer có nên switch Pokemon hay không
    /// (Có thể gọi từ BattleSystem nếu cần)
    /// </summary>
    public bool ShouldSwitchPokemon(Pokemon currentPokemon, Pokemon playerPokemon, PokemonParty trainerParty)
    {
        if (trainerParty == null || trainerParty.Pokemons.Count <= 1)
            return false;

        switch (personality)
        {
            case TrainerPersonality.Aggressive:
                // Switch nếu bị ưỡng nặng hoặc máu quá thấp
                float typeEff = CalculateTypeAdvantage(playerPokemon, currentPokemon);
                return typeEff > 1.5f || (float)currentPokemon.CurrentHp / currentPokemon.MaxHP < 0.2f;

            case TrainerPersonality.Tactical:
                // Switch nếu bị ưỡng hoặc máu dưới 30%
                typeEff = CalculateTypeAdvantage(playerPokemon, currentPokemon);
                return typeEff > 1f || (float)currentPokemon.CurrentHp / currentPokemon.MaxHP < 0.3f;

            case TrainerPersonality.Coward:
                // Switch nếu máu dưới 25% và có thay thế tốt hơn
                return (float)currentPokemon.CurrentHp / currentPokemon.MaxHP < 0.25f;

            default:
                return false;
        }
    }

    /// <summary>
    /// Tính lợi thế hệ giữa 2 Pokemon
    /// </summary>
    private float CalculateTypeAdvantage(Pokemon attacker, Pokemon defender)
    {
        float effect1 = TypeChart.GetEffectiveness(attacker.Base.Type1, defender.Base.Type1);
        float effect2 = TypeChart.GetEffectiveness(attacker.Base.Type1, defender.Base.Type2);
        return effect1 * effect2;
    }
}
