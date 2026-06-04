using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum định nghĩa tính cách của trainer NPC
/// </summary>
public enum TrainerPersonality
{
    Aggressive,  // Tấn công mạnh, ít hồi máu
    Tactical,    // Đọc type advantage, xét kế hoạch
    Coward       // Thủ nhiều, heal khi máu thấp
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
    }

    // ============ AGGRESSIVE TRAINER ============
    // Mục tiêu: Damage lớn nhất, ít dùng status
    private Move GetAggressiveMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -1f;

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0) continue;

            float score = EstimateDamagePotential(move, attacker, defender);
            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                // Aggressive trainer chỉ dùng status khi không có chiêu tấn công
                score = 0.1f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    // ============ TACTICAL TRAINER ============
    // Mục tiêu: Cân nhắc lợi thế hệ, tiết kiệm PP, dùng status hợp lý
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
                score += 2f * GetTypeEffectiveness(move, defender); // ưu tiên lợi thế hệ
                score *= 1f - (1f / (move.PP + 1f)); // giữ tài nguyên PP nếu không cần thiết
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    // ============ COWARD TRAINER ============
    // Mục tiêu: Máu thấp thì heal/buff, chọn chiêu an toàn
    private Move GetCowardMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -999f;
        float hpRatio = (float)attacker.CurrentHp / attacker.MaxHP;

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0) continue;

            float score;
            if (hpRatio <= 0.35f && move.Base.Target == MoveBase.MoveTartget.Self && move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 100f; // ưu tiên tự buff/heal khi máu thấp
            }
            else if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 10f + EvaluateStatusMove(move, attacker, defender);
            }
            else
            {
                float effectiveness = GetTypeEffectiveness(move, defender);
                score = effectiveness * 20f / (move.Base.Power + 10f); // chọn chiêu an toàn và hiệu quả
                if (effectiveness > 1f) score += 3f;
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
    }

    private float EvaluateStatusMove(Move move, Pokemon attacker, Pokemon defender)
    {
        float score = 0f;
        if (move.Base.Target == MoveBase.MoveTartget.Self)
        {
            if (move.Base.Effects != null && move.Base.Effects.Boosts != null && move.Base.Effects.Boosts.Count > 0)
            {
                score += 10f + move.Base.Effects.Boosts.Count * 2f;
            }
            if (move.Base.Effects != null && move.Base.Effects.Status != ConditionID.none)
            {
                score += 6f;
            }
        }
        else
        {
            if (move.Base.Effects != null && move.Base.Effects.Status != ConditionID.none)
            {
                score += 8f;
            }
            if (move.Base.Effects != null && move.Base.Effects.VolatileStatus != VolatileConditionID.none)
            {
                score += 5f;
            }
        }
        return score;
    }
}
