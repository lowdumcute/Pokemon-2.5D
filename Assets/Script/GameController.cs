using UnityEngine;
public enum GameState { FreeRoam, Battle}
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    [SerializeField] PlayerMovement playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;
    private void Start()
    {
        playerController.OnEncounted += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        playerController.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(false);
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindAnyObjectByType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    // Public API for starting a trainer battle from PlayerController
    public void StartTrainerBattle(PokemonParty playerParty, Pokemon trainerPokemon, TrainerPersonality personality)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        playerController.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(false);
        battleSystem.StartBattle(playerParty, trainerPokemon, personality);
    }

    private void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        playerController.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}
