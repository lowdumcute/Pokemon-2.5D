using UnityEngine;
using UnityEngine.UI;

public class MegaScreen : MonoBehaviour
{
    private Pokemon pokemon;
    [SerializeField] Image pokemonImage;
    [SerializeField] Image pokemonImagEVolution;
    public void Setup(Pokemon pokemon)
    {
        if (pokemon.Base.MegaEvolution != null &&
            pokemon.HeldItem != null &&
            pokemon.HeldItem.ItemName == pokemon.Base.MegaEvolution.requiredItem)
        {
            gameObject.SetActive(true);
            this.pokemon = pokemon;
            pokemonImage.sprite = pokemon.Base.FrontSprite;
            pokemonImagEVolution.sprite = pokemon.Base.MegaEvolution.megaForm.FrontSprite;
        }
        else
        {
            gameObject.SetActive(false);
            Debug.Log("Không đủ điều kiện để hiển thị MegaScreen");
        }
    }
}
