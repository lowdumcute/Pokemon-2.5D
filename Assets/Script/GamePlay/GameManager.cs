using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public EvolutionScreen evolutionScreen;
    public  Inventory inventory;
    [SerializeField] GameObject player;
    public TypeSprites typeSprites;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        evolutionScreen.gameObject.SetActive(false);
    }
}
