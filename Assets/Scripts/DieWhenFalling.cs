using UnityEngine;

public class DieWhenFalling : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y < -100) Destroy(gameObject);
    }
}
