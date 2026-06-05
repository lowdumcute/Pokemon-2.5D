using UnityEngine;

/// <summary>
/// Script cho NPC Trainer - sử dụng TrainerAI để chiến đấu với player
/// </summary>
public class NPCTrainer : MonoBehaviour
{
    [Header("Trainer Info")]
    [SerializeField] NPCData npcData;  // ScriptableObject chứa tên và thoại của trainer
    [SerializeField] NPCUI npcUI;      // Tham chiếu đến NPCUI để hiển thị tên và gọi UI
    [SerializeField] PokemonParty trainerParty;  // Party của trainer
    [SerializeField] float interactionRange = 2f;
    [SerializeField]private bool hasBeatenTrainer = false;  // Để tránh đánh lại NPC đã thắng

    public void Start()
    {
        npcUI.SetData(npcData);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Nếu player vào vùng trigger của trainer
        if (collision.collider.CompareTag("Player") && !hasBeatenTrainer)
        {
            StartBattleWithPlayer();
        }
        else if (collision.collider.CompareTag("Player") && hasBeatenTrainer)
        {
            Debug.Log($"You have already defeated {npcData.npcName}."); // sửa lại sau thành NPCData.defaultDialogues 
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

        Debug.Log($"{npcData.npcName} challenges you! (Personality: {npcData.personality})");

        // Request PlayerController to start the battle so GameController handles the transition
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.RequestStartTrainerBattle(trainerParty, trainerPokemon, npcData.personality);
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
            Debug.Log($"Defeated {npcData.npcName}!");
        }
        else
        {
            Debug.Log($"Lost to {npcData.npcName}!");
        }
    }

    // ===== Helper Methods =====

    /// <summary>
    /// Gán trainer party (nếu chưa gán trong Inspector)
    /// </summary>
    public void SetTrainerParty(PokemonParty party)
    {
        trainerParty = party;
    }

    public string GetTrainerName()
    {
        return npcData.npcName;
    }

    public bool HasBeenDefeated()
    {
        return hasBeatenTrainer;
    }
}
