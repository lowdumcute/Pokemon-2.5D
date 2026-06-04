using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] ExpBar expBar;
    [SerializeField] Color highlightedColor;
    [SerializeField] Image Icon;
    [SerializeField] Image type1;
    [SerializeField] Image type2;
    Pokemon _pokemon;
    public void setData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        // Xử lý type1
        if (_pokemon.Base.type1 != PokemonType.None)
        {
            type1.gameObject.SetActive(true);
            type1.sprite = GameManager.Instance.typeSprites.GetTypeSprite(_pokemon.Base.type1);
        }
        else
        {
            type1.gameObject.SetActive(false);
        }

        // Xử lý type2
        if (_pokemon.Base.type2 != PokemonType.None)
        {
            type2.gameObject.SetActive(true);
            type2.sprite = GameManager.Instance.typeSprites.GetTypeSprite(_pokemon.Base.type2);
        }
        else
        {
            type2.gameObject.SetActive(false);
        }

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv." + pokemon.Level.ToString();
        Icon.sprite = pokemon.Base.IconSprite;
        expBar.SetExp((float)pokemon.currentExp / pokemon.GetExpForLevel(pokemon.Level + 1));
        hpBar.SetHP((float)pokemon.CurrentHp / pokemon.MaxHP);
    }
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightedColor;
        }
        else
        {
            nameText.color = Color.black;
        }
    }
}
