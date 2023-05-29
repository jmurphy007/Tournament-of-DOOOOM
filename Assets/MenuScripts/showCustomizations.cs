using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class showCustomizations : MonoBehaviour
{
    public GameObject customizations, mainBtns;
    public Toggle randomColor;
    public TextMeshProUGUI changingTitle, buttonText, leftText, colorText, autoText;
    private bool showing = false;
    public static Color32[] customColors = new Color32[2];
    public Image[] selectedColors;
    public UICar car;
    public float time;
    public float maxTime;
    public Color32 newColor1, newColor2;

    // Start is called before the first frame update
    void Start()
    {
        customizations.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            leftText.text = "Left Handed? (L2)";
            colorText.text = "Random Colors? (L3)";
            autoText.text = "Automatic Gears? (R3)";
        } 
        else
        {
            leftText.text = "Left Handed?";
            colorText.text = "Random Colors?";
            autoText.text = "Automatic Gears?";
        }
        if (!randomColor.isOn)
        {
            customColors[0] = selectedColors[0].color;
            car.setColor(0, selectedColors[0].color);
            customColors[1] = selectedColors[1].color;
            car.setColor(1, selectedColors[1].color);
            time = 0;
        }
        else
        {
            if (time < maxTime)
            {
                time += Time.deltaTime;
                car.setColor(0, Color32.Lerp(car.CarBody.materials[0].color, newColor1, time));
                car.setColor(1, Color32.Lerp(car.Robot.materials[2].color, newColor1, time));
                car.setColor(0, Color32.Lerp(car.CarBody.materials[8].color, newColor2, time));
                car.setColor(1, Color32.Lerp(car.Robot.materials[1].color, newColor2, time));
            }
            else
            {
                time = 0;
                newColor1 = new Color32(
                (byte)Random.Range(0, 255),
                (byte)Random.Range(0, 255),
                (byte)Random.Range(0, 255),
                255);

                newColor2 = new Color32(
                    (byte)Random.Range(0, 255),
                    (byte)Random.Range(0, 255),
                    (byte)Random.Range(0, 255),
                    255);
            }
        }
    }

    public void ShowCustomization()
    {
        if (!showing)
        {
            showing = true;
            customizations.SetActive(true);
            mainBtns.SetActive(false);
            buttonText.text = "Close";
            changingTitle.text = "Car Settings";
        }
        else
        {
            showing = false;
            customizations.SetActive(false);
            mainBtns.SetActive(true);
            buttonText.text = "Car Settings";
            changingTitle.text = "Select a Level";
        }
    }
}
