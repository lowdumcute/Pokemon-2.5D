using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] private Option[] Options;

    private int currentSelection = 0;
    bool menuActive = true;
    bool OptionActive = false;
    void OnEnable()
    {
        playerMovement.enabled = false;
    }
    void OnDisable()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;
    }
    public void OpenScreen(int index)
    {
        if (index < 0 || index >= Options.Length)
            return;

        Options[index].ScreenOptions.SetActive(true);

        OptionActive = true;
        currentSelection = index;
    }
    void Update()
    {
        if (!menuActive) return;

        if (Keyboard.current.xKey.wasPressedThisFrame) // ấn x để thoát hoặc quay lại 
        {
            Debug.Log("Back Pressed");
            if (OptionActive)
            {
                Options[currentSelection].ScreenOptions.SetActive(false);
                PlayerController.Instance.IsAvailable = true;
                OptionActive = false;
            }
            else
            {
                gameObject.SetActive(false);
            }
            return;
        }
    }


    
    
    [System.Serializable]
    public class Option
    {
        public Button buttonOptions;
        public GameObject ScreenOptions;
    }
}
