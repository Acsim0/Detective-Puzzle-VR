using UnityEngine;

public class Seconcanvasscript : MonoBehaviour
{
    public GameObject secondCanvas;    
    public GameObject FinalCanvas;  

    public void SwitchCanvas()
    {
        secondCanvas.SetActive(false);
        FinalCanvas.SetActive(true);
    }
}
