using UnityEngine;

public class giveInt : MonoBehaviour
{
    public int giveWeapon;
    [SerializeField] private Renderer[] weapons;

    private void Start()
    {
        giveWeapon = Random.Range(0, weapons.Length);
        for (int i = 0; i < weapons.Length; i++)
        {
            if (i != giveWeapon)
            {
                weapons[i].enabled = false;
            }
            else
            {
                weapons[i].enabled = true;
            }
        }
    }
}