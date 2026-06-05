using System.Collections.Generic;
using UnityEngine;
using System.Text;
/// Enum định nghĩa tính cách của trainer NPC

public enum TrainerPersonality
{
    Aggressive,  // Tấn công mạnh, ít hồi máu, thích switch nếu bị ưỡng
    Tactical,    // Đọc type advantage, xét kế hoạch, cân bằng defense/offense
    Coward       // Thủ nhiều, heal khi máu thấp, switch nếu cần
}

/// Script xử lý logic chọn chiêu cho trainer battle
public class TrainerAI
{
    private TrainerPersonality personality;

    public TrainerAI(TrainerPersonality personality)
    {
        this.personality = personality;
    }

    /// Hàm chính để chọn move dựa trên tính cách trainer
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

    // AGGRESSIVE TRAINER 
    // Mục tiêu: Damage lớn nhất, nhưng vẫn xét lợi thế hệ, thích switch nếu bị ưỡng
    private Move GetAggressiveMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -1f;

        List<(string moveName, float score)> debugScores =
            new List<(string, float)>();

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0)
                continue;

            float score = EstimateDamagePotential(move, attacker, defender);
            float typeEff = GetTypeEffectiveness(move, defender);

            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 0.5f;
            }
            else
            {
                score += typeEff * 3f;

                if (defender.CurrentHp < defender.MaxHP * 0.2f)
                    score *= 1.5f;

                score += Random.Range(-2f, 2f);
            }

            debugScores.Add((move.Base.Name, score));

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        LogMoveScores(
            "AGGRESSIVE",
            attacker,
            debugScores,
            bestMove,
            bestScore);

        return bestMove;
    } 

    // TACTICAL TRAINER 
    // Mục tiêu: Cân nhắc lợi thế hệ, tiết kiệm PP, dùng status hợp lý, switch hợp lý
    private Move GetTacticalMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -999f;

        List<(string moveName, float score)> debugScores =
            new List<(string, float)>();

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0)
                continue;

            float score;

            if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = EvaluateStatusMove(
                    move,
                    attacker,
                    defender);
            }
            else
            {
                score = EstimateDamagePotential(
                    move,
                    attacker,
                    defender);

                float typeEff =
                    GetTypeEffectiveness(move, defender);

                score += 2.5f * typeEff;

                if (defender.CurrentHp < defender.MaxHP * 0.3f)
                    score *= 1.3f;

                score *= 1f - (1f / (move.PP + 1f));

                score += Random.Range(-1f, 1f);
            }

            debugScores.Add((move.Base.Name, score));

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        LogMoveScores(
            "TACTICAL",
            attacker,
            debugScores,
            bestMove,
            bestScore);

        return bestMove;
    }

    //  COWARD TRAINER 
    // Mục tiêu: Máu thấp thì heal/buff, chọn chiêu an toàn, switch nếu bị khắc chếchế
    private Move GetCowardMove(Pokemon attacker, Pokemon defender)
    {
        Move bestMove = attacker.Moves[0];
        float bestScore = -999f;

        float hpRatio =
            (float)attacker.CurrentHp / attacker.MaxHP;

        List<(string moveName, float score)> debugScores =
            new List<(string, float)>();

        foreach (var move in attacker.Moves)
        {
            if (move.PP <= 0)
                continue;

            float score;

            if (hpRatio <= 0.4f &&
                move.Base.Target == MoveBase.MoveTartget.Self &&
                move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 150f;
            }
            else if (hpRatio <= 0.6f &&
                    move.Base.Target == MoveBase.MoveTartget.Self &&
                    move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 80f;
            }
            else if (move.Base.Category == MoveBase.MoveCategory.Status)
            {
                score = 15f +
                    EvaluateStatusMove(
                        move,
                        attacker,
                        defender);
            }
            else
            {
                float effectiveness =
                    GetTypeEffectiveness(move, defender);

                score =
                    effectiveness > 1f ? 40f : 20f;

                score += effectiveness * 10f;

                if (defender.CurrentHp <
                    defender.MaxHP * 0.2f)
                {
                    score *= 1.2f;
                }

                score += Random.Range(-2f, 2f);
            }

            debugScores.Add((move.Base.Name, score));

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        LogMoveScores(
            "COWARD",
            attacker,
            debugScores,
            bestMove,
            bestScore);

        return bestMove;
    }

    // HELPER FUNCTIONS 

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
    // Helper để chọn Pokemon ra sân khi pokemon hiện tại bị hạ gục 
    public Pokemon ChooseBestPokemon(
    Pokemon playerPokemon,
    PokemonParty trainerParty)
    {
        Pokemon bestPokemon = null;
        float bestScore = float.MinValue;

        List<(string pokemonName, float score)> debugScores =
            new List<(string, float)>();

        foreach (var pokemon in trainerParty.Pokemons)
        {
            if (pokemon.CurrentHp <= 0) // hoặc pokemon.IsFainted()
                continue;

            float score = EvaluatePokemonSwitchScore(
                pokemon,
                playerPokemon);

            debugScores.Add((pokemon.Base.Name, score));

            if (score > bestScore)
            {
                bestScore = score;
                bestPokemon = pokemon;
            }
        }

        LogPokemonScores(
            personality.ToString(),
            debugScores,
            bestPokemon,
            bestScore);

        return bestPokemon;
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
                // Switch nếu bị khắc chếchế nặng hoặc máu quá thấp
                float typeEff = CalculateTypeAdvantage(playerPokemon, currentPokemon);
                return typeEff > 1.5f || (float)currentPokemon.CurrentHp / currentPokemon.MaxHP < 0.2f;

            case TrainerPersonality.Tactical:
                // Switch nếu bị khắc chế hoặc máu dưới 30%
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

    // Helper để log điểm số của từng move cho debug
    private void LogMoveScores(
        string aiType,
        Pokemon attacker,
        List<(string moveName, float score)> scores,
        Move selectedMove,
        float selectedScore)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"[{aiType}] {attacker.Base.Name}");

        for (int i = 0; i < scores.Count; i++)
        {
            sb.Append(
                $"Move {i + 1} ({scores[i].moveName}) => {scores[i].score:F1}");

            if (i < scores.Count - 1)
                sb.Append(" , ");
        }

        int chosenIndex = scores.FindIndex(
            x => x.moveName == selectedMove.Base.Name);

        sb.AppendLine();
        sb.Append(
            $"Choose Move {chosenIndex + 1} ({selectedMove.Base.Name}) => {selectedScore:F1}");

        Debug.Log(sb.ToString());
    }
    // Helper để log điểm số của từng Pokemon khi chọn switch
    private float EvaluatePokemonSwitchScore(
    Pokemon candidate,
    Pokemon playerPokemon)
    {
        float score = 0f;

        float hpRatio =
            (float)candidate.CurrentHp / candidate.MaxHP;

        float typeAdvantage =
            CalculateTypeAdvantage(candidate, playerPokemon);

        switch (personality)
        {
            case TrainerPersonality.Aggressive:

                score += typeAdvantage * 50f;

                score += hpRatio * 20f;

                break;

            case TrainerPersonality.Tactical:

                score += typeAdvantage * 70f;

                score += hpRatio * 30f;

                break;

            case TrainerPersonality.Coward:

                score += hpRatio * 80f;

                score += typeAdvantage * 20f;

                break;
        }

        score += Random.Range(-2f, 2f);

        return score;
    }
    private void LogPokemonScores(
    string aiType,
    List<(string pokemonName, float score)> scores,
    Pokemon selectedPokemon,
    float selectedScore)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"[{aiType}] Pokemon Selection");

        for (int i = 0; i < scores.Count; i++)
        {
            sb.Append(
                $"Pokemon {i + 1} ({scores[i].pokemonName}) => {scores[i].score:F1}");

            if (i < scores.Count - 1)
                sb.Append(" , ");
        }

        int chosenIndex =
            scores.FindIndex(
                x => x.pokemonName ==
                    selectedPokemon.Base.Name);

        sb.AppendLine();

        sb.Append(
            $"Choose Pokemon {chosenIndex + 1} ({selectedPokemon.Base.Name}) => {selectedScore:F1}");

        Debug.Log(sb.ToString());
    }
}
