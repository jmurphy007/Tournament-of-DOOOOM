using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class colorPicker : MonoBehaviour
{
    [SerializeField] private RectTransform texture;
    [SerializeField] private Image[] display;
    [SerializeField] private Texture2D sprite;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private TextMeshProUGUI[] currentSelection;
    public Color32 customColor;
    public bool carOrEnergy = false;

    private void Start()
    {
        currentSelection[0].enabled = false;
    }

    public void changeColorChoice()
    {
        if (carOrEnergy)
        {
            carOrEnergy = false;
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                buttonText.text = "Set for Car (R3)";
            } 
            else
            {
                buttonText.text = "Set for Car";
            }
            currentSelection[0].enabled = false;
            currentSelection[1].enabled = true;
        } 
        else
        {
            carOrEnergy = true;
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                buttonText.text = "Set for Energy (R3)";
            }
            else
            {
                buttonText.text = "Set for Energy";
            }
            currentSelection[0].enabled = true;
            currentSelection[1].enabled = false;
        }
    }

    public void onClickColorPicker()
    {
        SetColor();
    }

    private void SetColor()
    {
        Vector3 imagePos = texture.position;
        float globalX = Input.mousePosition.x - imagePos.x;
        float globalY = Input.mousePosition.y - imagePos.y;

        int localX = (int)(globalX * (sprite.width / texture.rect.width));
        int localY = (int)(globalY * (sprite.height / texture.rect.height));

        customColor = sprite.GetPixel(localX, localY);
        setActualColor();
    }

    private void setActualColor()
    {
        int i;
        if (carOrEnergy)
        {
            i = 0;
        } 
        else
        {
            i = 1;
        }

        display[i].color = customColor;
    }
}
