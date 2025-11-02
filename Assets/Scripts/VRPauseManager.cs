using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PersistentVRManager : MonoBehaviour
{

    // System wird nigends benutzt aufgrund von Komplikationen



    [SerializeField] private GameObject mainRig;
    [SerializeField] private Material titleSkybox;
    [SerializeField] private Material gameSkybox;

    private Dictionary<string, Scene> loadedLevels = new();
    private Scene? currentLevel;
    private Transform currentStartPoint;
    private Transform currentTitlePoint;
    private bool inMenu = true;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        
        LoadLevel("Level1"); 
        ShowTitlePoint();
    }

    

    public void LoadLevel(string levelName)
    {
        if (loadedLevels.ContainsKey(levelName)) return;

        SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive).completed += _ =>
        {
            var scene = SceneManager.GetSceneByName(levelName);
            loadedLevels[levelName] = scene;
            currentLevel = scene;

           
            FindPointsInScene(scene);

            
            SetActive(scene, false);
        };
    }

    public void UnloadLevel(string levelName)
    {
        if (!loadedLevels.ContainsKey(levelName)) return;
        SceneManager.UnloadSceneAsync(levelName);
        loadedLevels.Remove(levelName);
    }

    

    public Transform titlePoint; 

    private void FindPointsInScene(Scene scene)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "StartPoint")
                currentStartPoint = go.transform;
        }
    }


    public void ShowTitlePoint()
    {
        if (currentLevel == null) return;
        SetActive(currentLevel.Value, false);

        MoveRig(currentTitlePoint);
        RenderSettings.skybox = titleSkybox;
        inMenu = true;
        Time.timeScale = 0f;
    }

    public void StartOrResumeGame()
    {
        if (currentLevel == null) return;
        SetActive(currentLevel.Value, true);

        MoveRig(currentStartPoint);
        RenderSettings.skybox = gameSkybox;
        inMenu = false;
        Time.timeScale = 1f;
    }

    private void MoveRig(Transform point)
    {
        if (mainRig && point)
            mainRig.transform.SetPositionAndRotation(point.position, point.rotation);
    }

    private void SetActive(Scene scene, bool active)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name.Contains("Manager")) continue;
            go.SetActive(active);
        }
    }
}
