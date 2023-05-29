using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyHealth : MonoBehaviour
{
    public EnemyCarControl enemy;
    public GameObject player;
    float r, g, currentHealth, maxHealth;

    private void Start()
    {
        maxHealth = enemy.maxHealth;
    }

    void Update()
    {
        currentHealth = enemy.currentHealth;
        GetComponent<TextMesh>().text = Mathf.RoundToInt(currentHealth).ToString() + "/" + Mathf.RoundToInt(maxHealth).ToString() + "HP";

        if (currentHealth >= 2 * (maxHealth / 3))
        {
            r = 0f;
            g = 255f;
        } 
        else if (currentHealth >= maxHealth / 3)
        {
            r = 255f;
            g = 255f;
        } else
        {
            r = 255f;
            g = 0f;
        }

        Color32 fadeOut = new Color(r, g, 0f, 0f);
        Color32 fadeIn = new Color(r, g, 0f, 200f);

        GetComponent<TextMesh>().color = Color.Lerp(fadeOut, fadeIn, Mathf.Sin(Time.time * 2));

        transform.LookAt(player.transform);
    }
}
