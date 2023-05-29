using UnityEngine;

public class movingItem : MonoBehaviour
{
    private float range = 0.125f;
    private float current = 0f;
    private float direction = 0.125f;

    private float x, y, z;

    // Start is called before the first frame update
    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
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
        transform.Rotate(0, 0, 30 * Time.deltaTime);
    }
}