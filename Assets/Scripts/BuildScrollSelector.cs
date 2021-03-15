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
    GameObject currentSelecteCategory;

    [SerializeField]
    Image SelectionIcon;

    [SerializeField]
    int selectionIndex = 0;

    [SerializeField]
    int CycleIndex = 0;

    [SerializeField]
    List<UIItemDataContainer> weaponsSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<UIItemDataContainer> armourSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<UIItemDataContainer> engineSelections = new List<UIItemDataContainer>();

    [SerializeField]
    float ScrollFloat;

    [SerializeField]
    GameObject SelectedWeapon, SelectedArmour, SelectedEngine;


    void Start()
    {
        InitializeManualSelection();
    }

    public void InitializeManualSelection()
    {
        currentSelecteCategory = CursorSelection[selectionIndex].gameObject;

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
            CycleItem(false);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CycleItem(true);
        }
    }

    void CycleItem(bool isRight)
    {
        if (isRight)
            CycleIndex++;
        else
        {
            CycleIndex--;
            if (CycleIndex < 0)
            {
                switch (selectionIndex)
                {
                    case 0:
                        CycleIndex = weaponsSelections.Count - 1;
                        break;
                    case 1:
                        CycleIndex = armourSelections.Count - 1;
                        break;
                    case 2:
                        CycleIndex = engineSelections.Count - 1;
                        break;
                }
            }

        }

        switch (selectionIndex)
        {
            case 0:
                if (weaponsSelections.Count != 0)
                {
                    CycleIndex %= weaponsSelections.Count;
                    weaponsSelections[(int)CycleIndex].InjectButtonBuildDataToBuild();
                }
                break;
            case 1:
                if (armourSelections.Count != 0)
                {
                    CycleIndex %= armourSelections.Count;
                    armourSelections[(int)CycleIndex].InjectButtonBuildDataToBuild();
                }
                break;
            case 2:
                if (engineSelections.Count != 0)
                {
                    CycleIndex %= engineSelections.Count;
                    engineSelections[(int)CycleIndex].InjectButtonBuildDataToBuild();
                }
                break;
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
        CycleIndex = 0;
        currentSelecteCategory = CursorSelection[selectionIndex].gameObject;

        if (SelectionIcon != null)
        {
            SelectionIcon.rectTransform.anchoredPosition = CursorSelection[selectionIndex].gameObject.GetComponent<RectTransform>().anchoredPosition;
        }
    }
}
