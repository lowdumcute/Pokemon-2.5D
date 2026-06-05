using TMPro;
using UnityEngine;

public class NPCUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    public void SetData(NPCData data)
    {
        // Hiển thị tên NPC
        nameText.text = data.npcName;
    }
}