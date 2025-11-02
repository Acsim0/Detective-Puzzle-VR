using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnapManager : MonoBehaviour
{
    [System.Serializable]
    public class SnapSlot
    {
        public Transform point;
        [HideInInspector] public XRUIDraggableInteractable occupant;
        public string correctID;
    }

    public GameObject mainCanvas;    
    public GameObject secondCanvas;
    public List<SnapSlot> slots = new();
    public static SnapManager Instance;

    void Awake() => Instance = this;

    public bool TryOccupy(XRUIDraggableInteractable item, Transform targetPoint)
    {
        int targetIndex = slots.FindIndex(s => s.point == targetPoint);
        if (targetIndex < 0) return false;

        int oldIndex = slots.FindIndex(s => s.occupant == item);
        if (oldIndex >= 0)
            slots[oldIndex].occupant = null;

        if (slots[targetIndex].occupant == null)
        {
            slots[targetIndex].occupant = item;
            StartCoroutine(item.MoveToSmooth(slots[targetIndex].point.position));
            StartCoroutine(DelayedCheckSolution());
            return true;
        }

        int direction = 1;
        if (oldIndex >= 0 && targetIndex < oldIndex)
            direction = -1;

        bool success = ShiftAndWait(targetIndex, direction);

        if (success && slots[targetIndex].occupant == null)
        {
            slots[targetIndex].occupant = item;
            StartCoroutine(item.MoveToSmooth(slots[targetIndex].point.position));
            StartCoroutine(DelayedCheckSolution());
            return true;
        }

        return false;
    }


    /// Verschiebt Items zyklisch, d.h. vom Ende zurück zum Anfang.
    void ShiftWithWrap(int startIndex, int direction)
    {
        int count = slots.Count;
        if (direction > 0)
        {
            int last = (startIndex + count - 1) % count;
            XRUIDraggableInteractable carry = slots[last].occupant;

            for (int i = count - 1; i > 0; i--)
            {
                int from = (startIndex + i - 1) % count;
                int to = (startIndex + i) % count;
                slots[to].occupant = slots[from].occupant;
                if (slots[to].occupant != null)
                    StartCoroutine(slots[to].occupant.MoveToSmooth(slots[to].point.position));
            }

            slots[startIndex].occupant = carry;
            if (carry != null)
                StartCoroutine(carry.MoveToSmooth(slots[startIndex].point.position));
        }
        else
        {
            XRUIDraggableInteractable carry = slots[startIndex].occupant;
            for (int i = 0; i < slots.Count - 1; i++)
            {
                int from = (startIndex + i + 1) % count;
                int to = (startIndex + i) % count;
                slots[to].occupant = slots[from].occupant;
                if (slots[to].occupant != null)
                    StartCoroutine(slots[to].occupant.MoveToSmooth(slots[to].point.position));
            }

            int wrapIndex = (startIndex + slots.Count - 1) % count;
            slots[wrapIndex].occupant = carry;
            if (carry != null)
                StartCoroutine(carry.MoveToSmooth(slots[wrapIndex].point.position));
        }

        StartCoroutine(DelayedCheckSolution());
    }

    IEnumerator DelayedCheckSolution()
    {
        yield return new WaitForSeconds(0.3f);
        CheckSolution();
    }

    public bool CheckSolution()
    {
        bool allFilled = true;
        bool allCorrect = true;

        foreach (var s in slots)
        {
            if (s.occupant == null)
            {
                allFilled = false;
                allCorrect = false;
                continue;
            }

            if (s.occupant.itemID != s.correctID)
                allCorrect = false;
        }

        if (!allFilled)
        {
            return false;
        }

        if (allCorrect)
        {

            StartCoroutine(SolvedSequence(0.5f));
            return true;
        }

        return false;
    }

    private IEnumerator SolvedSequence(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var s in slots)
            if (s.occupant != null)
                s.occupant.gameObject.SetActive(false);

        if (mainCanvas != null) mainCanvas.SetActive(false);
        if (secondCanvas != null) secondCanvas.SetActive(true);

    }



    bool ShiftAndWait(int startIndex, int direction)
    {
        int count = slots.Count;
        int next = (startIndex + direction + count) % count;

        XRUIDraggableInteractable displaced = slots[startIndex].occupant;
        if (displaced == null) return true;

        if (slots[next].occupant != null)
            ShiftAndWait(next, direction);

        slots[next].occupant = displaced;
        slots[startIndex].occupant = null;
        StartCoroutine(displaced.MoveToSmooth(slots[next].point.position));
        return true;
    }

}
