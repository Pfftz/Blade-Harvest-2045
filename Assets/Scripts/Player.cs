using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public InventoryManager inventoryManager;
    private TileManager tileManager;

    private void Start()
    {
        tileManager = GameManager.instance.tileManager;
    }
    private void Awake()
    {
        inventoryManager = GetComponent<InventoryManager>();
    }
    private void Update()
    {
        if (tileManager != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3Int position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);

                string tileName = tileManager.GetTileName(position);

                if (!string.IsNullOrWhiteSpace(tileName))
                {
                    if (tileName == "Interactable" && inventoryManager.toolbar.selectedSlot.itemName == "Hoe")
                    {
                        tileManager.SetInteracted(position);
                    }
                }
            }
        }
    }
    public void DropItem(Item item)
    {
        Vector3 spawnLocation = transform.position;

        Vector3 spawnOffset = Random.insideUnitCircle * 1.25f;

        Item droppedItem = Instantiate(item, spawnLocation + spawnOffset, Quaternion.identity);

        droppedItem.rb.AddForce(spawnOffset * .2f, ForceMode2D.Impulse);
    }
    public void DropItem(Item item, int numToDrop)
    {
        for (int i = 0; i < numToDrop; i++)
        {
            DropItem(item);
        }
    }
}
