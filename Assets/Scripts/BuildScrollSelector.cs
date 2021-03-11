using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildScrollSelector : MonoBehaviour
{
    [SerializeField]
    List<GameObject> CursorSelection = new List<GameObject>();

    [SerializeField]
    List<GameObject> LootObjectContainers = new List<GameObject>();

    [SerializeField]
    GameObject currentSelection;

    [SerializeField]
    Image SelectionIcon;

    int selectionIndex = 0;

    float scrollConstant = 0;

    [SerializeField]
    List<UIItemDataContainer> weaponsSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<UIItemDataContainer> armourSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<UIItemDataContainer> engineSelections = new List<UIItemDataContainer>();


    public void ScrollToTop()
    {
        //currentSelection.GetComponent<ScrollRect>().horizontalNormalizedPosition += scrollConstant;

        //currentSelection.GetComponent<ScrollRect>().horizontalNormalizedPosition %= 1f;


        currentSelection.GetComponent<ScrollRect>().content.localPosition = 
            GetSnapToPositionToBringChildIntoView(currentSelection.GetComponent<ScrollRect>(),
            currentSelection.transform.GetChild(0).GetComponent<RectTransform>());
    }
    public void ScrollToBottom()
    {
        //currentSelection.GetComponent<ScrollRect>().horizontalNormalizedPosition -= scrollConstant;

        //currentSelection.GetComponent<ScrollRect>().horizontalNormalizedPosition %= 1f;
        currentSelection.GetComponent<ScrollRect>().content.localPosition =
    GetSnapToPositionToBringChildIntoView(currentSelection.GetComponent<ScrollRect>(),
    currentSelection.transform.GetChild(currentSelection.transform.childCount).GetComponent<RectTransform>());
    }

    void Start()
    {
        InitializeManualSelection();

        //for (int i = 0; i < CursorSelection.Count; i++)
        //{
        //    CursorSelection[i].gameObject.GetComponent<ScrollRect>().horizontalNormalizedPosition = 0f;
        //}
    }

    public void InitializeManualSelection()
    {
        currentSelection = CursorSelection[selectionIndex].gameObject;

        if (SelectionIcon != null)
        {
            SelectionIcon.rectTransform.anchoredPosition = CursorSelection[selectionIndex].gameObject.GetComponent<RectTransform>().anchoredPosition;
        }

        //Populate Weapons
        for (int i = 0; i < LootObjectContainers[0].transform.childCount; i++)
        {
            weaponsSelections.Add(LootObjectContainers[0].transform.GetChild(i).GetComponent<UIItemDataContainer>());
        }

        //Populate Armour
        for (int i = 0; i < LootObjectContainers[1].transform.childCount; i++)
        {
            armourSelections.Add(LootObjectContainers[1].transform.GetChild(i).GetComponent<UIItemDataContainer>());
        }

        //Populate Engine
        for (int i = 0; i < LootObjectContainers[2].transform.childCount; i++)
        {
            engineSelections.Add(LootObjectContainers[2].transform.GetChild(i).GetComponent<UIItemDataContainer>());
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SwitchCollectionType(true);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SwitchCollectionType(false);
        }


        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ScrollToBottom();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ScrollToTop();
        }
    }

    void SwitchCollectionType(bool isDown)
    {
        if (isDown)
        {
            selectionIndex++;
        }
        else
        {
            selectionIndex--;

            if (selectionIndex < 0)
            {
                selectionIndex = 2;
            }
        }
        selectionIndex %= CursorSelection.Count;
        currentSelection = CursorSelection[selectionIndex].gameObject;

        if (SelectionIcon != null)
        {
            SelectionIcon.rectTransform.anchoredPosition = CursorSelection[selectionIndex].gameObject.GetComponent<RectTransform>().anchoredPosition;
        }
    }

    public static Vector2 GetSnapToPositionToBringChildIntoView(ScrollRect instance, RectTransform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return result;
    }
}
