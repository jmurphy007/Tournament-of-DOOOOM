using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spikes : MonoBehaviour
{
    public  float range = .75f;
    public float current = 0f;
    public float direction = 2.5f;

    private float x, y, z;

    // Start is called before the first frame update
    void Start()
    {
        x = transform.position.x;
        y = transform.position.y - 0.35f;
        z = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        current += Time.deltaTime * direction;
        if (current >= range)
        {
            direction *= -1;
            current = range;
        }
        else if (current <= (-(range)))
        {
            direction *= -1;
            current = -(range);
        }
        transform.position = new Vector3(x, (y + current), z);
    }
}
