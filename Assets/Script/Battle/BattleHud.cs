using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] public TMP_Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] ExpBar expBar;
    [SerializeField] Image Type1Image;
    [SerializeField] Image Type2Image;
    [SerializeField] Image statusIcon;
    Pokemon _pokemon;
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        SetLevel();

        hpBar.SetHP((float)pokemon.CurrentHp / pokemon.MaxHP);
        expBar.SetExp((float)pokemon.currentExp / _pokemon.GetExpForLevel(_pokemon.Level + 1));

        // ⚡ Cập nhật icon hệ Type1
        if (pokemon.Base.Type1 != PokemonType.None)
        {
            Type1Image.gameObject.SetActive(true);
            Type1Image.sprite = GameManager.Instance.typeSprites.GetTypeSprite(pokemon.Base.Type1);
        }
        else
        {
            Type1Image.gameObject.SetActive(false);
        }

        // ⚡ Cập nhật icon hệ Type2
        if (pokemon.Base.Type2 != PokemonType.None && pokemon.Base.Type2 != pokemon.Base.Type1)
        {
            Type2Image.gameObject.SetActive(true);
            Type2Image.sprite = GameManager.Instance.typeSprites.GetTypeSprite(pokemon.Base.Type2);
        }
        else
        {
            Type2Image.gameObject.SetActive(false);
        }

        // ⚠ Cập nhật icon trạng thái nếu có
        if (pokemon.Status != null)
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.sprite = GameManager.Instance.typeSprites.GetStatusSprite(pokemon.StatusID);
        }
        else
        {
            statusIcon.gameObject.SetActive(false);
        }
    }
    public IEnumerator UpdateUI()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.CurrentHp / _pokemon.MaxHP);
            _pokemon.HpChanged = false;
        }
        if (_pokemon.ExpChanged)
        {
            yield return expBar.SetExpSmooth((float)_pokemon.currentExp / _pokemon.GetExpForLevel(_pokemon.Level + 1));
            Debug.Log($"{_pokemon.currentExp}/ {_pokemon.GetExpForLevel(_pokemon.Level + 1)}");
            _pokemon.ExpChanged = false;
        }
        // ⚠ Cập nhật icon trạng thái nếu có
        if (_pokemon.Status != null)
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.sprite = GameManager.Instance.typeSprites.GetStatusSprite(_pokemon.StatusID);
        }
        else
        {
            statusIcon.gameObject.SetActive(false);
        }
    }
    public void SetLevel()
    {
        levelText.text = "Lv." + _pokemon.Level.ToString();
    }
}
