using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StarterPokemonReceiver : MonoBehaviour
{
    [Header("Pokemon")]
    [SerializeField] private PokemonBase[] chosenStarterBase; // 3 Pokémon khởi đầu
    [SerializeField] private int starterLevel = 1;
    [SerializeField] private Image currentStarterChose; // Hiển thị Pokémon đang chọn
    [Header("UI")]
    [SerializeField] private GameObject choiceScreen;
    [SerializeField] private Button[] choiceButtons; // Gán 3 nút
    [SerializeField] private Button acceptButton; // Nút Accept
    [SerializeField] private Animator animator;
    private int currentChosenIndex = -1;

    private void Start()
    {
        choiceScreen.SetActive(false);
        currentStarterChose.gameObject.SetActive(false);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            choiceButtons[i].onClick.AddListener(() => OnChooseButtonClicked(index));
        }

        acceptButton.onClick.AddListener(AcceptChosenPokemon);
    }

    private void Update()
    {
        if (!choiceScreen.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            AcceptChosenPokemon();
        }
    }

    public void ShowChoice()
    {
        choiceScreen.SetActive(true);

        // Reset trạng thái hiển thị và lựa chọn khi mở lại
        currentChosenIndex = -1;
        currentStarterChose.gameObject.SetActive(false);

        // Reset tất cả Select về false
        foreach (var btn in choiceButtons)
        {
            var select = btn.GetComponent<Select>();
            if (select != null)
                select.SetSelect(false);
        }
    }

    private void MoveSelection(int direction)
    {
        if (chosenStarterBase.Length == 0) return;

        if (currentChosenIndex == -1)
        {
            // Nếu chưa chọn gì, bắt đầu chọn từ 0 hoặc cuối tùy theo hướng
            currentChosenIndex = direction > 0 ? 0 : chosenStarterBase.Length - 1;
        }
        else
        {
            currentChosenIndex += direction;

            // Vòng lại nếu vượt giới hạn
            if (currentChosenIndex >= chosenStarterBase.Length)
                currentChosenIndex = 0;
            else if (currentChosenIndex < 0)
                currentChosenIndex = chosenStarterBase.Length - 1;
        }

        OnChooseButtonClicked(currentChosenIndex);
    }

    private void OnChooseButtonClicked(int index)
    {
        currentStarterChose.gameObject.SetActive(true);
        currentChosenIndex = index;
        currentStarterChose.sprite = chosenStarterBase[index].FrontSprite;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            var select = choiceButtons[i].GetComponent<Select>();
            if (select != null)
                select.SetSelect(i == index);
        }
    }

    private void AcceptChosenPokemon()
    {
        if (currentChosenIndex < 0 || currentChosenIndex >= chosenStarterBase.Length)
        {
            Debug.LogWarning("Chưa chọn Pokémon khởi đầu hợp lệ.");
            return;
        }

        var party = FindAnyObjectByType<PokemonParty>();
        if (party == null)
        {
            Debug.LogError("Không tìm thấy PokemonParty trong scene.");
            return;
        }

        if (party.Pokemons.Count == 0)
        {
            Pokemon starterPokemon = new Pokemon(chosenStarterBase[currentChosenIndex], starterLevel,chosenStarterBase[currentChosenIndex].MaxHP); 
            party.Pokemons.Add(starterPokemon);
            starterPokemon.Init();

            Debug.Log($"Đã chọn Pokémon khởi đầu: {starterPokemon.Base.Name}");
        }
        else
        {
            Debug.Log("Party đã có Pokémon, không thêm lại.");
        }
        StartCoroutine(ShowResult());
    }
    private IEnumerator ShowResult()
    {
        animator.SetTrigger("ShowResult");
        yield return new WaitForSeconds(3f);
        choiceScreen.SetActive(false);
    }
}
