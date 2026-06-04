using UnityEngine;

/// <summary>
/// Script cho NPC Trainer - sử dụng TrainerAI để chiến đấu với player
/// </summary>
public class NPCTrainer : MonoBehaviour
{
    [SerializeField] string trainerName = "Trainer";
    [SerializeField] TrainerPersonality personality = TrainerPersonality.Tactical;
    [SerializeField] PokemonParty trainerParty;  // Party của trainer
    [SerializeField] float interactionRange = 2f;
    
    private bool hasBeatenTrainer = false;  // Để tránh đánh lại NPC đã thắng


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu player vào vùng trigger của trainer (2D physics)
        if (collision.CompareTag("Player") && !hasBeatenTrainer)
        {
            StartBattleWithPlayer();
        }
    }

    public void StartBattleWithPlayer()
    {
        // Lấy player party từ PlayerMovement hoặc PlayerController
        var playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement == null) return;

        var playerParty = playerMovement.GetComponent<PokemonParty>();
        if (playerParty == null || trainerParty == null) return;

        // Bắt đầu trainer battle với TrainerPersonality
        // Lấy Pokemon khỏe từ trainer party
        var trainerPokemon = trainerParty.GetHealthyPokemon();
        if (trainerPokemon == null) return;

        Debug.Log($"{trainerName} challenges you! (Personality: {personality})");

        // Request PlayerController to start the battle so GameController handles the transition
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.RequestStartTrainerBattle(trainerParty, trainerPokemon, personality);
            if (BattleSystem.Instance != null)
                BattleSystem.Instance.OnBattleOver += OnBattleResult;
        }
        else
        {
            Debug.LogWarning("PlayerController.Instance is null - cannot request trainer battle");
        }
    }

    private void OnBattleResult(bool playerWon)
    {
        BattleSystem.Instance.OnBattleOver -= OnBattleResult;

        // Re-enable player movement and camera
        var playerMovementObj = FindAnyObjectByType<PlayerMovement>();
        if (playerMovementObj != null)
        {
            playerMovementObj.gameObject.SetActive(true);
        }

        if (Camera.main != null)
            Camera.main.gameObject.SetActive(true);

        if (playerWon)
        {
            hasBeatenTrainer = true;
            Debug.Log($"Defeated {trainerName}!");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Lost to {trainerName}!");
        }
    }

    // ===== Helper Methods =====
    
    /// <summary>
    /// Gán personality cho trainer qua code
    /// </summary>
    public void SetPersonality(TrainerPersonality newPersonality)
    {
        personality = newPersonality;
    }

    /// <summary>
    /// Gán trainer party (nếu chưa gán trong Inspector)
    /// </summary>
    public void SetTrainerParty(PokemonParty party)
    {
        trainerParty = party;
    }

    public string GetTrainerName()
    {
        return trainerName;
    }

    public bool HasBeenDefeated()
    {
        return hasBeatenTrainer;
    }
}
