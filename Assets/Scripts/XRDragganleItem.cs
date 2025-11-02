using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class XRUIDraggableInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    [Header("Follow Settings")]
    public float followSpeed = 12f;

    [Header("Snap Settings")]
    public float snapSpeed = 14f;
    public float snapDistance = 0.4f;
    public Transform[] snapPoints;

    [Header("Identification")]
    public string itemID;                 

    [HideInInspector] public RectTransform rect;
    [HideInInspector] public float canvasZ;

    private Transform interactor;
    private bool isDragging;
    private Coroutine snapRoutine;

    protected override void Awake()
    {
        base.Awake();

        rect = GetComponent<RectTransform>();
        var canvas = GetComponentInParent<Canvas>();
        canvasZ = canvas ? canvas.transform.position.z : 0f;

        selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;

        
        TrySnapToNearestOnStart();
    }

    private void Start()
    {
        StartCoroutine(DelayedAutoSnap());
    }

    private IEnumerator DelayedAutoSnap()
    {
        yield return new WaitForSeconds(0.1f);
        TrySnapToNearestOnStart();
    }

    private void TrySnapToNearestOnStart()
    {
        if (snapPoints == null || snapPoints.Length == 0) return;

        Transform nearest = null;
        float min = float.MaxValue;

        foreach (var p in snapPoints)
        {
            if (!p) continue;
            float d = Vector3.Distance(transform.position, p.position);
            if (d < min) { min = d; nearest = p; }
        }

        if (nearest != null && min <= snapDistance)
        {
            if (SnapManager.Instance != null)
                SnapManager.Instance.TryOccupy(this, nearest);
            else
                StartCoroutine(SnapTo(nearest.position));
        }
    }

    
    void Update()
    {
        if (!isDragging || interactor == null) return;

        var canvas = rect.GetComponentInParent<Canvas>();
        var canvasTransform = canvas.transform;
        var plane = new Plane(canvasTransform.forward, canvasTransform.position);

        if (plane.Raycast(new Ray(interactor.position, interactor.forward), out float dist))
        {
            Vector3 hitPoint = interactor.position + interactor.forward * dist;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);

            Vector3 local = canvasRect.InverseTransformPoint(hitPoint);
            Vector3 min = canvasRect.InverseTransformPoint(corners[0]);
            Vector3 max = canvasRect.InverseTransformPoint(corners[2]);

            local.x = Mathf.Clamp(local.x, min.x, max.x);
            local.y = Mathf.Clamp(local.y, min.y, max.y);

            Vector3 clampedWorld = canvasRect.TransformPoint(local);
            rect.position = Vector3.Lerp(rect.position, clampedWorld, Time.deltaTime * followSpeed);
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        interactor = args.interactorObject.transform;
        isDragging = true;

        if (snapRoutine != null) { StopCoroutine(snapRoutine); snapRoutine = null; }

        if (SnapManager.Instance != null)
        {
            foreach (var s in SnapManager.Instance.slots)
            {
                if (s.occupant == this)
                {
                    s.occupant = null;
                    break;
                }
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        isDragging = false;
        interactor = null;
        TrySnapToNearest();

        if (SnapManager.Instance != null)
            SnapManager.Instance.CheckSolution();
    }

    private void TrySnapToNearest()
    {
        if (SnapManager.Instance == null) return;

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var slot in SnapManager.Instance.slots)
        {
            if (slot.point == null) continue;

            float dist = Vector3.Distance(transform.position, slot.point.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = slot.point;
            }
        }

        if (nearest != null && minDist <= snapDistance)
        {
            
            SnapManager.Instance.TryOccupy(this, nearest);

            
            if (snapRoutine != null)
                StopCoroutine(snapRoutine);

            
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                Vector3 localTarget = canvasRect.InverseTransformPoint(nearest.position);
                localTarget.z = 0f;

                snapRoutine = StartCoroutine(MoveToSmoothLocal(localTarget));
            }
            else
            {
                
                snapRoutine = StartCoroutine(MoveToSmooth(nearest.position));
            }
        }
    }


    public IEnumerator SnapTo(Vector3 target)
    {
        target.z = rect.position.z;
        while (Vector3.Distance(rect.position, target) > 0.001f)
        {
            rect.position = Vector3.Lerp(rect.position, target, Time.deltaTime * snapSpeed);
            yield return null;
        }
        rect.position = target;
    }

    public IEnumerator MoveToSmooth(Vector3 targetWorldPos)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();

        
        if (!canvas)
        {
            Vector3 start = rect.position;
            float t = 0f;
            float duration = 0.25f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float smooth = Mathf.SmoothStep(0, 1, t);
                rect.position = Vector3.Lerp(start, targetWorldPos, smooth);
                yield return null;
            }

            rect.position = targetWorldPos;
            rect.localScale = Vector3.one * 0.25f;
            yield break;
        }

        
        Vector3 localTarget;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        localTarget = canvasRect.InverseTransformPoint(targetWorldPos);
        localTarget.z = 0f; 

        Vector3 localStart = rect.localPosition;
        float durationLerp = 0.25f;
        float tLerp = 0f;

        while (tLerp < 1f)
        {
            tLerp += Time.deltaTime / durationLerp;
            float smooth = Mathf.SmoothStep(0, 1, tLerp);
            rect.localPosition = Vector3.Lerp(localStart, localTarget, smooth);
            yield return null;
        }

        rect.localPosition = localTarget;
        rect.localScale = Vector3.one * 0.25f;
    }

    public IEnumerator MoveToSmoothLocal(Vector3 localTarget)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 localStart = rect.localPosition;
        float duration = 0.25f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smooth = Mathf.SmoothStep(0, 1, t);
            rect.localPosition = Vector3.Lerp(localStart, localTarget, smooth);
            yield return null;
        }

        rect.localPosition = localTarget;
        rect.localScale = Vector3.one * 0.25f;
    }




}
