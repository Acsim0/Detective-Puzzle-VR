using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;

public class CutsceneTriggerXR : MonoBehaviour
{
    [Header("Cutscenes")]
    public PlayableDirector mainCutscene;      
    public PlayableDirector secondaryCutscene; 

    [Header("XR Settings")]
    public string rightHandName = "Right Controller";

    [Header("Freizuschaltende Bilder (erst nach Secondary)")]
    public List<XRUIDraggableInteractable> draggableItems;
    public List<string> activateIDs;

    private bool rightHandInside = false;
    private bool mainCutscenePlayed = false;

    // Globale aktive Zone für die GoldWatch
    public static CutsceneTriggerXR CurrentZone { get; private set; }

    private void Start()
    {
        if (mainCutscene != null) mainCutscene.stopped += OnCutsceneStopped;
        if (secondaryCutscene != null) secondaryCutscene.stopped += OnCutsceneStopped;
    }

    private void OnDestroy()
    {
        if (mainCutscene != null) mainCutscene.stopped -= OnCutsceneStopped;
        if (secondaryCutscene != null) secondaryCutscene.stopped -= OnCutsceneStopped;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.name.Contains(rightHandName)) return;

        rightHandInside = true;
        CurrentZone = this; 

        if (mainCutscene != null && !mainCutscenePlayed)
        {
            mainCutscene.time = 0;
            mainCutscene.Play();
            mainCutscenePlayed = true;
           
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.name.Contains(rightHandName)) return;

        rightHandInside = false;
        if (CurrentZone == this) CurrentZone = null; 
    }

    public bool IsRightHandInside() => rightHandInside;

    public void PlaySecondaryCutscene()
    {
        if (secondaryCutscene == null) return;
        secondaryCutscene.time = 0;
        secondaryCutscene.Play();
        
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        if (director == secondaryCutscene)
        {
            ActivateDraggableImages();
        }

        StartCoroutine(CleanupAfterEnd(director));
    }

    private void ActivateDraggableImages()
    {
        if (draggableItems == null || activateIDs == null) return;

        foreach (var item in draggableItems)
        {
            if (item != null && activateIDs.Contains(item.itemID))
            {
                item.gameObject.SetActive(true);
                
            }
        }
    }

    private IEnumerator CleanupAfterEnd(PlayableDirector d)
    {
        yield return null;
        d.Stop();
        d.time = 0;
        d.Evaluate();

        foreach (var kvp in d.playableAsset.outputs)
        {
            var go = d.GetGenericBinding(kvp.sourceObject) as GameObject;
            if (go) go.SetActive(false);
        }

      
    }
}
