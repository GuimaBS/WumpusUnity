using UnityEngine;
using UnityEngine.EventSystems;

public class ArrastarPainel : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public RectTransform painelParaMover;
    private Vector2 offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            painelParaMover,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            painelParaMover.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localMousePosition))
        {
            painelParaMover.localPosition = localMousePosition - offset;
        }
    }
}
