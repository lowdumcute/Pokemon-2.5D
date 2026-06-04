using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color colorUnselected;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject dialogBox; // Thêm dialogBox để có thể bật/tắt toàn bộ hộp thoại
    [SerializeField] List<Button> actionButtons;
    [SerializeField] List<Image> moveTypeImage; // mỗi Image là parent chứa NameMove và PP
    [SerializeField] TypeSprites typeSprites;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogBox.SetActive(true); // Bật hộp thoại khi bắt đầu gõ
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
        dialogBox.SetActive(false); // Tắt hộp thoại sau khi gõ xong
    }

    public void EnableDialogText(bool enable)
    {
        dialogText.gameObject.SetActive(enable);
    }

    public void EnableActionSelector(bool enable)
    {
        actionSelector.SetActive(enable);
    }

    public void EnableMoveSelector(bool enable)
    {
        moveSelector.SetActive(enable);
    }

    public void UpdateActionSelection(int selectAction)
    {
        for (int i = 0; i < actionButtons.Count; ++i)
        {
            if (i == selectAction)
            {
                // Chọn button này
                actionButtons[i].Select();  // ép Unity hiển thị sprite đã set trong Sprite Swap
            }
            else
            {
                // Bỏ chọn button này
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == actionButtons[i].gameObject)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveTypeImage.Count; ++i)
        {
            if (moveTypeImage[i].gameObject.activeSelf)
            {
                bool isSelected = i == selectedMove;

                // Chỉ đổi màu
                moveTypeImage[i].color = isSelected ? Color.white : colorUnselected;

                // Chỉ thay đổi vị trí Y nếu được chọn
                Vector3 pos = moveTypeImage[i].transform.localPosition;
                pos.y = isSelected ? 20f : 0f;
                moveTypeImage[i].transform.localPosition = pos;

                // Cập nhật PP
                var ppText = moveTypeImage[i].transform.GetChild(1)?.GetComponent<TMP_Text>();
                if (ppText != null)
                    ppText.text = isSelected ? $"{move.PP}/{move.Base.PP}" : "";
            }
        }
    }


    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTypeImage.Count; ++i)
        {
            bool hasMove = i < moves.Count;
            moveTypeImage[i].gameObject.SetActive(hasMove);

            if (hasMove)
            {
                var move = moves[i];

                var nameText = moveTypeImage[i].transform.GetChild(0)?.GetComponent<TMP_Text>();
                var ppText = moveTypeImage[i].transform.GetChild(1)?.GetComponent<TMP_Text>();

                if (nameText != null)
                    nameText.text = move.Base.Name;

                if (ppText != null)
                    ppText.text = $"{move.PP}/{move.Base.PP}";

                // Set sprite theo type MỘT LẦN ở đây
                moveTypeImage[i].sprite = typeSprites.GetMoveSprite(move.Base.Type);

                // Default màu không được chọn
                moveTypeImage[i].color = colorUnselected;
            }
        }
    }

}
