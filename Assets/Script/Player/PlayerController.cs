using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [HideInInspector] public PlayerSystem inputActions;

    [Header("Screen")]
    [SerializeField] private GameObject MainMenu;

    public bool IsAvailable = true;
    public PartyScreen partyScreen;
    public PokemonParty playerParty;

    public Vector2 MoveInput { get; private set; }

    private GameObject interactableObject;

    private void Awake()
    {
        MainMenu.SetActive(false);

        inputActions = new PlayerSystem();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (IsAvailable)
        {
            MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        }
        else
        {
            MoveInput = Vector2.zero;
        }

        if (inputActions.Player.OpenBag.triggered)
        {
            ToggleMainMenuScreen();
        }

        if (inputActions.Player.Interact.triggered)
        {
            if (interactableObject != null)
            {
                var receiver =
                    interactableObject.GetComponent<StarterPokemonReceiver>();

                if (receiver != null)
                {
                    receiver.ShowChoice();
                }
            }
        }
    }    public void RequestStartTrainerBattle(
        PokemonParty trainerParty,
        Pokemon trainerPokemon,
        TrainerPersonality personality)
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.StartTrainerBattle(
                playerParty,
                trainerParty,
                trainerPokemon,
                personality);
        }
    }
    // add vào party button ở main menu 
    public void OpenPartyScreen()
    {
        IsAvailable = false;

        partyScreen.SetPartyData(playerParty.Pokemons);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interact"))
        {
            interactableObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == interactableObject)
        {
            interactableObject = null;
        }
    }

    private void ToggleMainMenuScreen()
    {
        if (!IsAvailable)
            return;

        MainMenu.SetActive(!MainMenu.activeSelf);

        Debug.Log(MainMenu.activeSelf
            ? "Open Bag"
            : "Close Bag");
    }
}