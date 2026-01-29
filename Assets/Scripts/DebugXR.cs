using UnityEngine;
using UnityEngine.EventSystems;

public class DebugXRUIButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[UI] Pointer ENTER from: {eventData.pointerId}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[UI] Pointer EXIT from: {eventData.pointerId}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[UI] CLICK from: {eventData.pointerId}");
    }
}
