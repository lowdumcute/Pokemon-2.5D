using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class EvolutionScreen : MonoBehaviour
{
    [SerializeField] TMP_Text messText;
    [SerializeField] Image pokemonImage;
    [SerializeField] Image PokemonEvolutionImage;
    string pokemonName;
    string pokemonEvolutioName;
    public void SetData(Pokemon pokemon)
    {
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        PokemonEvolutionImage.sprite = pokemon.Base.Evolution.evolvedForm.FrontSprite;
        //name 
        pokemonName = pokemon.Base.Name;
        pokemonEvolutioName = pokemon.Base.Evolution.evolvedForm.Name;
    }
    public void OnEnable()
    {
        StartCoroutine(ShowevolutionScreen());
    }
    public IEnumerator ShowevolutionScreen()
    {
        yield return StartCoroutine(TypeDialog($"What? {pokemonName} is evolving!"));
        yield return new WaitForSeconds(4.5f);

        yield return StartCoroutine(TypeDialog($"Congratulations! Your {pokemonName} evolved into {pokemonEvolutioName}"));
        yield return new WaitForSeconds(1.5f);

        gameObject.SetActive(false); // tắt lại nếu muốn
    }
    public IEnumerator TypeDialog(string dialog)
    {
        messText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            messText.text += letter;
            yield return new WaitForSeconds(1f/ 30f);
        }
        yield return new WaitForSeconds(1f);
    }
}
