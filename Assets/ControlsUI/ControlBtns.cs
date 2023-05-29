using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlBtns : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image[] controls; // 0 for Keyboard, 1 for G29 Wheel
    [SerializeField] private TextMeshProUGUI[] controlText; // 0 for Keyboard, 1 for G29 Wheel
    public GameObject ControlsScreen;
    private bool controlBool = false;
    public bool controlScreen = false;

    public void controlsScreen(bool visible)
    {
        ControlsScreen.SetActive(visible);
        controlScreen = visible;
        updateControlDisplay();
    }

    public void updateControlDisplay()
    {
        if (controlBool)
        {
            controlBool = false;
            buttonText.text = "KEYBOARD";
            controls[0].enabled = false;
            controlText[0].enabled = false;
            controls[1].enabled = true;
            controlText[1].enabled = true;
        }
        else
        {
            controlBool = true;
            buttonText.text = "LOGITECH G29";
            controls[0].enabled = true;
            controlText[0].enabled = true;
            controls[1].enabled = false;
            controlText[1].enabled = false;
        }
    }
}
