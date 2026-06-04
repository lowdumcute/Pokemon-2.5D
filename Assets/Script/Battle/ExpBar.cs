using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    [SerializeField] private Image expFill;         // Image có type = Filled
    [SerializeField] private float updateSpeed = 1f; // Tốc độ tăng

    public void SetExp(float normalizedExp)
    {
        // Đặt thẳng nếu không cần hiệu ứng
        expFill.fillAmount = normalizedExp;
    }

    public IEnumerator SetExpSmooth(float normalizedExp)
    {
        float preFill = expFill.fillAmount;

        // Nếu tăng nhiều, sẽ mượt hơn
        while (Mathf.Abs(preFill - normalizedExp) > 0.01f)
        {
            preFill = Mathf.MoveTowards(preFill, normalizedExp, updateSpeed * Time.deltaTime);
            expFill.fillAmount = preFill;
            yield return null;
        }

        expFill.fillAmount = normalizedExp; // Đảm bảo đúng giá trị cuối cùng
    }

    public void ResetExp()
    {
        expFill.fillAmount = 0f;
    }
}
