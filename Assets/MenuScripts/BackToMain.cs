using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToMain : MonoBehaviour
{
    public LevelLoader loader;
    public void mainMenu()
    {
        Time.timeScale = 1;
        loader.LoadLevel(0);
    }

    private void Start()
    {
        print(LogitechGSDK.LogiSteeringInitialize(false));
    }

    private void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            if (LogitechGSDK.LogiButtonTriggered(0, 23))
            {
                mainMenu();
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                mainMenu();
            }
        }
    }
}
