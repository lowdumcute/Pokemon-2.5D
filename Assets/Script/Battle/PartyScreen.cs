    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;

    public class PartyScreen : MonoBehaviour
    {
        [SerializeField] TMP_Text mesageText;
        [SerializeField] PartyMemberUI[] memberSlots;
        List<Pokemon> pokemons;
        public bool isUseItem;
        public void Init()
        {
            memberSlots = GetComponentsInChildren<PartyMemberUI>(includeInactive: true);
        }
        public void SetPartyData(List<Pokemon> pokemons)
        {
            this.pokemons = pokemons;
            for (int i = 0; i < memberSlots.Length; i++)
            {
                if (i < pokemons.Count)
                {
                    memberSlots[i].gameObject.SetActive(true); 
                    memberSlots[i].setData(pokemons[i]);
                    
                }
                else
                {
                    memberSlots[i].gameObject.SetActive(false);
                }
                mesageText.text = "Choose a Pokemon";
            }
        }
        
       public void UpdateMemberSelection(int selectedMember)
        {
            // Đảm bảo không vượt quá số lượng slot
            for (int i = 0; i < memberSlots.Length; i++)
            {
                if (i < pokemons.Count)
                {
                    memberSlots[i].SetSelected(i == selectedMember);
                }
                else
                {
                    memberSlots[i].SetSelected(false);
                }
            }
        }
        public void SetMessageText(string message)
        {
            mesageText.text = message;
        }
        public void Refresh(List<Pokemon> updatedPokemons)
        {
            SetPartyData(updatedPokemons);
        }
    }
