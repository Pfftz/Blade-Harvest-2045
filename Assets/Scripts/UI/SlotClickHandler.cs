using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Add this component to slot GameObjects to enable click selection.
/// This component should be added alongside Button component or used with EventTrigger.
/// </summary>
public class SlotClickHandler : MonoBehaviour, IPointerClickHandler
{
    private Slots_UI slotUI;

    private void Awake()
    {
        slotUI = GetComponent<Slots_UI>();
        if (slotUI == null)
        {
            Debug.LogError("SlotClickHandler requires a Slots_UI component on the same GameObject!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slotUI != null)
        {
            slotUI.OnSlotClick();
        }
    }

    // Alternative method for Button onClick events
    public void OnButtonClick()
    {
        if (slotUI != null)
        {
            slotUI.OnSlotClick();
        }
    }
}
