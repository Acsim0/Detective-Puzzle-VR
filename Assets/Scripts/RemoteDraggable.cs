using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRUIDraggableItem : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    [Header("Snap Settings")]
    public RectTransform[] snapPoints;
    public float snapDistance = 0.3f;
    public float followSpeed = 10f;

    private Transform currentInteractor;
    private RectTransform rectTransform;
    private bool isDragging = false;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentInteractor = args.interactorObject.transform;
        isDragging = true;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        isDragging = false;
        currentInteractor = null;
        TrySnapToClosestPoint();
    }

    private void Update()
    {
        if (isDragging && currentInteractor != null)
        {
            
            Vector3 targetPos = currentInteractor.position;
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPos, Time.deltaTime * followSpeed);
        }
    }

    private void TrySnapToClosestPoint()
    {
        if (snapPoints == null || snapPoints.Length == 0)
            return;

        RectTransform closest = null;
        float minDist = float.MaxValue;

        foreach (var point in snapPoints)
        {
            float dist = Vector3.Distance(rectTransform.position, point.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = point;
            }
        }

        if (closest != null && minDist < snapDistance)
        {
            rectTransform.position = closest.position;
        }
    }
}