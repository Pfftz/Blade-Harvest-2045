using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toolbar_UI : MonoBehaviour
{
    [SerializeField] private List<Slots_UI> toolbarSlots = new List<Slots_UI>();

    private Slots_UI selectedSlot;
    private void Start()
    {
        SelectSlot(0); // Select the first slot by default
    }
    private void Update()
    {
        if (toolbarSlots.Count == 10)
        {
            CheckAlphaNumericKeys();
        }
    }
    public void SelectSlot(int index)
    {
        if (toolbarSlots.Count == 10)
        {
            if(selectedSlot != null)
            {
                selectedSlot.SetHighlight(false); // Deselect the previous slot
            }
            selectedSlot = toolbarSlots[index];
            selectedSlot.SetHighlight(true); // Highlight the selected slot
        }
    }
    private void CheckAlphaNumericKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSlot(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SelectSlot(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SelectSlot(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SelectSlot(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SelectSlot(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SelectSlot(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectSlot(9);
        }
    }

}
