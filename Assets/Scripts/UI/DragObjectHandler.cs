using UnityEngine;
using UnityEngine.EventSystems;

public class DragObjectHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject placeholderGO;
    GameObject placeholder;
    [HideInInspector]
    public Transform origParent;
    [HideInInspector]
    public Transform placeholderParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        placeholder = Instantiate(placeholderGO);
        placeholder.transform.SetParent(transform.parent);

        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        origParent = transform.parent;
        placeholderParent = origParent;
        transform.SetParent(transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        if (placeholder.transform.parent != placeholderParent) placeholder.transform.SetParent(placeholderParent);

        int newSiblingIndex = placeholderParent.childCount;

        for (int i = 0; i < placeholderParent.childCount; i++)
        {
            if (transform.position.x < placeholderParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;
                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex) newSiblingIndex--;
                break;
            }
        }
        placeholder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(origParent);
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        Destroy(placeholder);
    }
}
