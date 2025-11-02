using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform Kamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Kamera = Kamera.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Kamera.position;
    }
}
