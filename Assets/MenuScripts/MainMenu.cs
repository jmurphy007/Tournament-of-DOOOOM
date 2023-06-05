using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public LevelLoader loader;
    public ControlBtns controls;
    public showCustomizations showCustom;
    public colorPicker colorPick;
    public AudioSource backgroundMusic, controlsMusic;
    public GameObject menuVisuals, levelSelect, controlBtnImgs, levelBns, customize, howToScr, lvlSelect;
    public float waitTime;
    public Image Logo;
    private Color32 visible, invisible;
    public static bool leftHand = false;
    public static bool randomColor = false;
    public static bool auto = false;
    public Toggle colors, left, carAuto;

    public void leftHanded()
    {
        if (leftHand)
        {
            leftHand = false;
        } 
        else
        {
            leftHand = true;
        }
    }

    public void randomColors()
    {
        if (randomColor)
        {
            randomColor = false;
        }
        else
        {
            randomColor = true;
        }
    }
    public void autoCar()
    {
        if (auto)
        {
            auto = false;
        }
        else
        {
            auto = true;
        }
    }

    IEnumerator loopMenu()
    {
        yield return new WaitForSeconds(223f);

        back();
    }

    private void Start()
    {
        invisible = new Color(255f, 255f, 255f, 0f);
        visible = new Color(255f, 255f, 255f, 200f);
        Logo.color = invisible;
        menuVisuals.SetActive(false);
        levelSelect.SetActive(false);
        controlBtnImgs.SetActive(false);
        controls.controlsScreen(false);
        controlsMusic.Stop();
        StartCoroutine(showmainMenu());
        StartCoroutine(loopMenu());
        print(LogitechGSDK.LogiSteeringInitialize(false));
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnApplicationQuit()
    {
        LogitechGSDK.LogiSteeringShutdown();
    }

    IEnumerator showmainMenu()
    {
        yield return new WaitForSeconds(waitTime);
        menuVisuals.SetActive(true);
        controlBtnImgs.SetActive(true);
        Logo.enabled = false;
    }

    public void instructions(bool visible)
    {
        controlBtnImgs.SetActive(!visible);
        controls.controlsScreen(visible);
        if (visible)
        {
            controlsMusic.Play();
            backgroundMusic.Pause();
        }
        else
        {
            controlsMusic.Stop();
            backgroundMusic.UnPause();
        }
    }

    public void levelSelectScreen()
    {
        levelSelect.SetActive(true);
        menuVisuals.SetActive(false);
        lvlSelect.SetActive(false);
    }

    public void howTo()
    {
        howToScr.SetActive(false);
        lvlSelect.SetActive(true);
    }

    public void selectLevel(int level)
    {
        loader.LoadLevel(level);
    }

    public void back()
    {
        loader.LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void exit()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Logo.enabled)
        {
            Logo.color = Color.Lerp(invisible, visible, Mathf.Sin(Time.time * 1.125f));
        }
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            if (LogitechGSDK.LogiButtonIsPressed(0, 23))
            {
                if (levelSelect.activeInHierarchy == false)
                {
                    exit();
                }
                else
                {
                    back();
                }
            }

            if (LogitechGSDK.LogiButtonReleased(0, 1))
            {
                if (howToScr.activeInHierarchy == false)
                {
                    if (levelSelect.activeInHierarchy == false)
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            selectLevel(1);
                        }
                    }
                    else
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(2);
                        }
                    }
                }
            }

            if (LogitechGSDK.LogiButtonReleased(0, 3))
            {
                if (howToScr.activeInHierarchy == false)
                {
                    if (levelSelect.activeInHierarchy == false)
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            levelSelectScreen();
                        }
                    }
                    else
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(3);
                        }
                    }
                }
            }

            if (LogitechGSDK.LogiButtonReleased(0, 2))
            {
                if (howToScr.activeInHierarchy == true)
                {
                    howTo();
                }
                else
                {
                    if (levelSelect.activeInHierarchy == true)
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(4);
                        }
                    }
                    else
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            instructions(true);
                        }
                        else
                        {
                            instructions(false);
                        }
                    }
                }
            }

            if (LogitechGSDK.LogiButtonReleased(0, 0))
            {
                if (howToScr.activeInHierarchy == false)
                {
                    if (levelSelect.activeInHierarchy == false)
                    {
                        if (controls.ControlsScreen.activeInHierarchy == true)
                        {
                            controls.updateControlDisplay();
                        }
                        else
                        {
                            exit();
                        }
                    }
                    else
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(5);
                        }
                    }
                }
            }

            if (howToScr.activeInHierarchy == false)
            {
                if (LogitechGSDK.LogiButtonReleased(0, 21) || LogitechGSDK.LogiButtonReleased(0, 22))
                {
                    showCustom.ShowCustomization();
                }

                if (LogitechGSDK.LogiButtonReleased(0, 7))
                {
                    if (left.isOn)
                    {
                        left.isOn = false;
                    }
                    else
                    {
                        left.isOn = true;
                    }
                }
                if (LogitechGSDK.LogiButtonReleased(0, 11))
                {
                    if (colors.isOn)
                    {
                        colors.isOn = false;
                    }
                    else
                    {
                        colors.isOn = true;
                    }
                }
                if (LogitechGSDK.LogiButtonReleased(0, 6))
                {
                    if (carAuto.isOn)
                    {
                        carAuto.isOn = false;
                    }
                    else
                    {
                        carAuto.isOn = true;
                    }
                }

                if (LogitechGSDK.LogiButtonReleased(0, 10))
                {
                    if (customize.activeInHierarchy)
                    {
                        colorPick.changeColorChoice();
                    }
                }
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (levelSelect.activeInHierarchy == false)
                {
                    exit();
                }
                else
                {
                    back();
                }
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                if (howToScr.activeInHierarchy == false)
                {
                    if (levelSelect.activeInHierarchy == false)
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            selectLevel(1);
                        }
                    }
                    else
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(2);
                        }
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                if (howToScr.activeInHierarchy == false)
                {
                    if (levelSelect.activeInHierarchy == false)
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            levelSelectScreen();
                        }
                    }
                    else
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(3);
                        }
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                if (howToScr.activeInHierarchy == true)
                {
                    howTo();
                }
                else
                {
                    if (levelSelect.activeInHierarchy == true)
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(4);
                        }
                    }
                    else
                    {
                        if (controls.ControlsScreen.activeInHierarchy == false)
                        {
                            instructions(true);
                        }
                        else
                        {
                            instructions(false);
                        }
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                if (howToScr.activeInHierarchy == false)
                {
                    if (levelSelect.activeInHierarchy == false)
                    {
                        if (controls.ControlsScreen.activeInHierarchy == true)
                        {
                            controls.updateControlDisplay();
                        }
                        else
                        {
                            exit();
                        }
                    }
                    else
                    {
                        if (!customize.activeInHierarchy)
                        {
                            selectLevel(5);
                        }
                    }
                }
            }
        }
    }
}
