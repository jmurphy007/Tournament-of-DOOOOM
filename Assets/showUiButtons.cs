using UnityEngine;

public class showUiButtons : MonoBehaviour
{
    public GameObject[] buttons; // 0 for Keyboard, 1 For G29 Wheel
    
    void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            buttons[0].SetActive(false);
            buttons[1].SetActive(true);
        }
        else
        {
            buttons[0].SetActive(true);
            buttons[1].SetActive(false);
        }
    }
}
