using UnityEngine;

public class Snappoints : MonoBehaviour
{


    public int rows = 2;
    public int cols = 2;
    public float spacing = 1f; 
    public RectTransform[] snapPoints; 

    private void Awake()
    {
        snapPoints = new RectTransform[rows * cols];

        int index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Snap Point erstellen
                GameObject go = new GameObject($"SnapPoint_{r}_{c}");
                go.transform.SetParent(transform); 
                RectTransform rt = go.AddComponent<RectTransform>();

                
                float x = (c - (cols - 1) / 2f) * spacing;
                float y = ((rows - 1) / 2f - r) * spacing;
                rt.localPosition = new Vector3(x, y, 0);

                snapPoints[index] = rt; 
                index++;
            }
        }
    }
}


