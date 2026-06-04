using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] PartyScreen partyScreen;
    PokemonParty playerParty;
    void OpenPartyScreen()
    {
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }
}
