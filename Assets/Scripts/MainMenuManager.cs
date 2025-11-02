using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject optionsPanel; 

    [Header("Scene Name to Load")]
    [SerializeField] private string gameSceneName = "MainVRScene"; 

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayPressed);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitPressed);
        if (optionsButton != null && optionsPanel != null)
            optionsButton.onClick.AddListener(OnOptionsPressed);

        Transform rig = GameObject.Find("XR Origin (PauseRig)").transform; 
        float distance = 4f;                                    


        transform.position = rig.position + rig.forward * distance;

        Vector3 lookPos = rig.position;
        lookPos.y = transform.position.y; 
        transform.LookAt(lookPos);
        transform.Rotate(0, 180f, 0);
        transform.position = rig.position + rig.forward * distance + rig.up * 1.6f + rig.right * 0.5f;
    }




    private void OnPlayPressed()
    {
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void OnQuitPressed()
    {
        Application.Quit();
    }

    private void OnOptionsPressed()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }
}
