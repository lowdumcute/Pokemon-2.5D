using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image healthImageFront; 
    [SerializeField] private Image healthImageBack; 
    [SerializeField] private float smoothSpeed = 10f;  // Tốc độ giảm máu thật
    [SerializeField] private float backDelay = 0.3f;  // Độ trễ trước khi thanh sau bắt đầu tuột

    public void SetHP(float hpNormalized)
    {
        hpNormalized = Mathf.Clamp01(hpNormalized);
        healthImageFront.fillAmount = hpNormalized;
        healthImageBack.fillAmount = hpNormalized;
    }

    public IEnumerator SetHPSmooth(float targetNormalizedHP)
    {
        targetNormalizedHP = Mathf.Clamp01(targetNormalizedHP);

        // Front bar giảm ngay
        while (Mathf.Abs(healthImageFront.fillAmount - targetNormalizedHP) > 0.001f)
        {
            healthImageFront.fillAmount = Mathf.Lerp(healthImageFront.fillAmount, targetNormalizedHP, Time.deltaTime * smoothSpeed * 2f);
            yield return null;
        }
        healthImageFront.fillAmount = targetNormalizedHP;

        // Delay trước khi back bar giảm
        yield return new WaitForSeconds(backDelay);

        // Back bar tuột từ từ
        while (Mathf.Abs(healthImageBack.fillAmount - targetNormalizedHP) > 0.001f)
        {
            healthImageBack.fillAmount = Mathf.Lerp(healthImageBack.fillAmount, targetNormalizedHP, Time.deltaTime * smoothSpeed);
            yield return null;
        }
        healthImageBack.fillAmount = targetNormalizedHP;
    }
}
