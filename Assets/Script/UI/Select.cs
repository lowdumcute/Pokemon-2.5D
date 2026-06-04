using UnityEngine;

public class Select : MonoBehaviour
{
    [SerializeField] GameObject SelectImage;
    public bool IsSelect;

    void Start()
    {
        IsSelect = false;
        SelectImage.SetActive(false);
    }

    // Hàm công khai để set giá trị IsSelect nếu cần
    public void SetSelect(bool value)
    {
        IsSelect = value;
        SelectImage.SetActive(IsSelect);
    }
}
