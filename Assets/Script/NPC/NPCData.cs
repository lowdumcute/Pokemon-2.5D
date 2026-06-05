using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("Info")]
    public string npcName;
    public TrainerPersonality personality;

    [TextArea(3, 5)]
    public string[] defaultDialogues; // Các câu thoại mặc định khi nói chuyện với NPC
}