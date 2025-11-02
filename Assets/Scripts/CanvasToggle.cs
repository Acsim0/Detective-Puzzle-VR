using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CanvasToggle : MonoBehaviour
{
    [Header("Refs")]
    public XRGrabInteractable grab;          
    public GameObject canvasObject;          
    public GameObject draggableContainer;   
    public Transform playerHead;             
    public Animator bookAnimator;           

    [Header("Behavior")]
    public bool requireHeldToToggle = true; 
    public float distanceFromPlayer = 0.7f;  
    public float heightOffset = -0.1f;       
    public float toggleCooldown = 0.2f;      

    private bool isHeld = false;
    private bool isOpen = false;
    private float lastToggleTime = -999f;

    private void OnEnable()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        if (!playerHead && Camera.main) playerHead = Camera.main.transform;

        if (grab)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
            grab.activated.AddListener(OnActivated);
        }
    }

    private void OnDisable()
    {
        if (grab)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
            grab.activated.RemoveListener(OnActivated);
        }
    }

    private void OnGrab(SelectEnterEventArgs _)
    {
        isHeld = true;
    }

    private void OnRelease(SelectExitEventArgs _)
    {
        isHeld = false;

        if (isOpen)
            CloseCanvas();
    }

    private void OnActivated(ActivateEventArgs _)
    {
        if (requireHeldToToggle && !isHeld) return;
        if (Time.time - lastToggleTime < toggleCooldown) return;

        lastToggleTime = Time.time;
        if (isOpen) CloseCanvas();
        else OpenCanvas();
    }

    private void OpenCanvas()
    {
        if (!canvasObject || !playerHead)
        {
            return;
        }

        
        if (bookAnimator) bookAnimator.SetTrigger("Open");

       
        PositionCanvasInFrontOfPlayer();

       
        SetVisible(canvasObject, true);
        if (draggableContainer) SetVisible(draggableContainer, true);

        isOpen = true;
    }

    private void CloseCanvas()
    {
        if (bookAnimator) bookAnimator.SetTrigger("Close");

        SetVisible(canvasObject, false);
        if (draggableContainer) SetVisible(draggableContainer, false);

        isOpen = false;
    }

    private void SetVisible(GameObject obj, bool visible)
    {
        var group = obj.GetComponent<CanvasGroup>();
        if (!group) group = obj.AddComponent<CanvasGroup>();

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private void PositionCanvasInFrontOfPlayer()
    {
        
        Vector3 flatForward = new Vector3(playerHead.forward.x, 0f, playerHead.forward.z).normalized;
        if (flatForward.sqrMagnitude < 0.0001f) flatForward = playerHead.forward.normalized;

        Vector3 pos = playerHead.position + flatForward * distanceFromPlayer;
        pos.y += heightOffset;

        
        Vector3 lookDir = (playerHead.position - pos);
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude < 0.0001f) lookDir = -flatForward;

        Quaternion rot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        rot *= Quaternion.Euler(0f, 180f, 0f);

        canvasObject.transform.SetPositionAndRotation(pos, rot);
    }
}
