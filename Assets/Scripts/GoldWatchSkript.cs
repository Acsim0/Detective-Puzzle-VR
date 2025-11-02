using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GoldWatchScript : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    [Header("Animator Params")]
    public string isHeldParam = "IsHeldBool";
    public string defaultTriggerWhenActivated = "Backward"; 

    private int isHeldHash;
    private bool isHeld;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>(true);
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        isHeldHash = Animator.StringToHash(isHeldParam);
    }

    private void OnEnable()
    {
        if (grab == null) return;
        grab.selectEntered.AddListener(OnPickedUp);
        grab.selectExited.AddListener(OnReleased);
        grab.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        if (grab == null) return;
        grab.selectEntered.RemoveListener(OnPickedUp);
        grab.selectExited.RemoveListener(OnReleased);
        grab.activated.RemoveListener(OnActivated);
    }

    private void OnPickedUp(SelectEnterEventArgs _)
    {
        isHeld = true;
        if (animator) animator.SetBool(isHeldHash, true);
    }

    private void OnReleased(SelectExitEventArgs _)
    {
        isHeld = false;
        if (animator) animator.SetBool(isHeldHash, false);
    }

    private void OnActivated(ActivateEventArgs _)
    {
        if (!isHeld) return;

        var zone = CutsceneTriggerXR.CurrentZone;
        if (zone == null || !zone.IsRightHandInside()) return;

       
        if (animator)
            animator.SetTrigger(defaultTriggerWhenActivated);
    }

   
    public void OnGoldWatchAnimationEvent()
    {
        CutsceneTriggerXR.CurrentZone?.PlaySecondaryCutscene();
    }
}
