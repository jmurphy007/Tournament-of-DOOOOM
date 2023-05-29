using UnityEngine;

public class UICar : MonoBehaviour
{
    public Renderer CarBody, Robot;
    [SerializeField] private GameObject[] Tires;

    public void setColor(int index, Color32 color)
    {
        if (index == 0)
        {
            materialInstance(CarBody, 0, color, false);
            materialInstance(Robot, 2, color, false);
        } 
        else
        {
            materialInstance(CarBody, 8, color, true);
            materialInstance(Robot, 1, color, true);

            int[] tireElements = { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34 };
            for (int i = 0; i < Tires.Length; i++)
            {
                for (int j = 0; j < tireElements.Length; j++)
                {
                    materialInstance(Tires[i].GetComponent<Renderer>(), tireElements[j], color, true);
                }
            }
        }
    }

    public void materialInstance(Renderer rend, int index, Color32 color, bool emission)
    {
        Material[] materials = rend.materials;
        if (emission == true)
        {
            materials[index].SetColor("_EmissionColor", color);
        }
        materials[index].SetColor("_Color", color);
        rend.materials = materials;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 15 * Time.deltaTime, 0);
        foreach (GameObject i in Tires)
        {
            i.transform.Rotate(0, 0, 60 * Time.deltaTime);
        }
    }
}
