using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemImage
    : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
{
    public Image image;
    public ItemStack currentItem;
    public InventoryUI inventoryUI;
    public GameObject itemAmountText;

    [HideInInspector]
    public Transform parentAfterDrag;

    private UiArea _uiArea;
    public UiArea UiArea
    {
        get { return _uiArea; }
        set { _uiArea = value; }
    }



    private void Start()
    {
        GameObject inventoryUIObject = GameObject.Find("InventoryUI");
        inventoryUI = inventoryUIObject.GetComponent<InventoryUI>();
        itemAmountText = transform.GetChild(0).gameObject;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        itemAmountText.SetActive(false);
        if (inventoryUI.itemTitle == null)
        {
            Destroy(inventoryUI.itemTitle);
            inventoryUI.itemTitle = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
        itemAmountText.SetActive(true);
        if (inventoryUI.itemTitle != null)
        {
            Destroy(inventoryUI.itemTitle);
            inventoryUI.itemTitle = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (
            eventData.button == PointerEventData.InputButton.Right
            && currentItem.item is CollectibleObject
        )
        {
            inventoryUI.DecreaseItemAmount(this,2);
            inventoryUI.RemoveAllItems();
            inventoryUI.Player.UIController.LoadItems();
            (currentItem.item as CollectibleObject).Use(inventoryUI.Player);
        }
    }
}
