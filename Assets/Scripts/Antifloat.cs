using UnityEngine;

public class Antifloat : MonoBehaviour
{
   
    void Update()
    {
        transform.position = new Vector3(transform.position.x, 0.15f, transform.position.z);
    }
}
