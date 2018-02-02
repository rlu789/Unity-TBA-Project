using UnityEngine;
using UnityEngine.EventSystems;

public class DropObjectHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int zone;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DragObjectHandler d = eventData.pointerDrag.GetComponent<DragObjectHandler>();

        if (d != null) d.placeholderParent = transform;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DragObjectHandler d = eventData.pointerDrag.GetComponent<DragObjectHandler>();

        if (d != null && d.placeholderParent == transform) d.placeholderParent = d.origParent;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DragObjectHandler d = eventData.pointerDrag.GetComponent<DragObjectHandler>();

        if (d != null) d.origParent = transform;
        CardUI card = d.GetComponent<CardUI>();
        if (zone != card.zone)
        {
            card.ChangeZone(zone, true);
        }
    }
}
