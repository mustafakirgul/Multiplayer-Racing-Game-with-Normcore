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
    int selectionIndex = 3;

    [SerializeField]
    int CycleIndex = 0;

    [SerializeField]
    List<UIItemDataContainer> weaponsSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<UIItemDataContainer> armourSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<UIItemDataContainer> engineSelections = new List<UIItemDataContainer>();

    [SerializeField]
    List<GameObject> carBuild = new List<GameObject>();

    [SerializeField]
    float ScrollFloat;

    [SerializeField]
    GameObject SelectedWeapon, SelectedArmour, SelectedEngine;

    [SerializeField]
    private UIManager uIManager;

    [SerializeField]
    private Text WeaponIndex, WeaponTotalCount;

    [SerializeField]
    private Text ArmourIndex, ArmourTotalCount;

    [SerializeField]
    private Text EngineIndex, EngineTotalCount;

    [SerializeField]
    private Text buildIndex, BuildTotalCount;

    public CarPhysicsParamsSObj currentSelectBuild;

    [SerializeField]
    private VisualStatsManager VisualStatsManager;

    private LootManager lootManager;

    [SerializeField]
    GameObject ConnectMenu;
    void Start()
    {
        lootManager = FindObjectOfType<LootManager>();
        UpdateFromTemplateData();
        InitializeManualSelection();

        StartCoroutine(WaitToInitialize());
    }
    private IEnumerator WaitToInitialize()
    {
        yield return new WaitForSeconds(1f);
        if (VisualStatsManager != null)
        {
            currentSelectBuild = lootManager.CarPhysicsParams[0];
            VisualStatsManager.UpdateMaximums();
            VisualStatsManager.SetVisualStats(currentSelectBuild);
        }
    }

    private void UpdateFromTemplateData()
    {
        for (int i = 0; i < lootManager.CarPhysicsParams.Count; i++)
        {
            lootManager.CarPhysicsParams[i].ResetData(
                lootManager.CarPhysicsTemplates[i]);
        }

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
            if (!weaponsSelections.Contains(LootObjectContainers[0].transform.GetChild(i).GetComponent<UIItemDataContainer>()))
                weaponsSelections.Add(LootObjectContainers[0].transform.GetChild(i).GetComponent<UIItemDataContainer>());
        }

        //Populate Armour
        for (int i = 0; i < LootObjectContainers[1].transform.childCount; i++)
        {
            if (!armourSelections.Contains(LootObjectContainers[1].transform.GetChild(i).GetComponent<UIItemDataContainer>()))
                armourSelections.Add(LootObjectContainers[1].transform.GetChild(i).GetComponent<UIItemDataContainer>());
        }

        //Populate Engine
        for (int i = 0; i < LootObjectContainers[2].transform.childCount; i++)
        {
            if (!engineSelections.Contains(LootObjectContainers[2].transform.GetChild(i).GetComponent<UIItemDataContainer>()))
                engineSelections.Add(LootObjectContainers[2].transform.GetChild(i).GetComponent<UIItemDataContainer>());
        }

        //Populate Builds
        for (int i = 0; i < LootObjectContainers[3].transform.childCount; i++)
        {
            if (!carBuild.Contains(LootObjectContainers[3].transform.GetChild(i).gameObject))
                carBuild.Add(LootObjectContainers[3].transform.GetChild(i).gameObject);
        }

        PopulateUIIndicators();
    }


    private void PopulateUIIndicators()
    {
        WeaponIndex.text = "1";
        ArmourIndex.text = "1";
        EngineIndex.text = "1";
        buildIndex.text = "1";

        if (weaponsSelections.Count != 0)
        {
            WeaponTotalCount.text = weaponsSelections.Count.ToString();
        }
        else
        {
            WeaponTotalCount.text = "1";
        }


        if (armourSelections.Count != 0)
        {
            ArmourTotalCount.text = armourSelections.Count.ToString();
        }
        else
        {
            ArmourTotalCount.text = "1";
        }

        if (engineSelections.Count != 0)
        {
            EngineTotalCount.text = engineSelections.Count.ToString();
        }
        else
        {
            EngineTotalCount.text = "1";
        }


        if (carBuild.Count != 0)
        {
            BuildTotalCount.text = carBuild.Count.ToString();
        }
        else
        {
            BuildTotalCount.text = "1";
        }
    }
    private void Update()
    {
        if (!ConnectMenu.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.S))
            {
                SwitchCollectionType(true);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.W))
            {
                SwitchCollectionType(false);
            }


            if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.A))
            {
                CycleItem(false);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.D))
            {
                CycleItem(true);
            }
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
                    case 3:
                        CycleIndex = carBuild.Count - 1;
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

                    VisualStatsManager.SetVisualStats(currentSelectBuild);
                    //Need to update the current ParamsObj with the data from the loot piece to have it reflected in the menu stats bar
                    //when switching items need to reset back the data that was touched
                    //Note this is done in the UIItemDataContainer now


                    //Display Relevant Stats Data Here
                    WeaponIndex.text = (CycleIndex + 1).ToString();
                }
                break;
            case 1:
                if (armourSelections.Count != 0)
                {
                    CycleIndex %= armourSelections.Count;
                    armourSelections[(int)CycleIndex].InjectButtonBuildDataToBuild();

                    VisualStatsManager.SetVisualStats(currentSelectBuild);
                    //Need to update the current ParamsObj with the data from the loot piece to have it reflected in the menu stats bar
                    //when switching items need to reset back the data that was touched
                    //Note this is done in the UIItemDataContainer now


                    //Display Relevant Stats Data Here
                    ArmourIndex.text = (CycleIndex + 1).ToString();
                }
                break;
            case 2:
                if (engineSelections.Count != 0)
                {
                    CycleIndex %= engineSelections.Count;
                    engineSelections[(int)CycleIndex].InjectButtonBuildDataToBuild();

                    VisualStatsManager.SetVisualStats(currentSelectBuild);

                    //Need to update the current ParamsObj with the data from the loot piece to have it reflected in the menu stats bar
                    //when switching items need to reset back the data that was touched
                    //Note this is done in the UIItemDataContainer now

                    //Display Relevant Stats Data Here
                    EngineIndex.text = (CycleIndex + 1).ToString();
                }
                break;
            case 3:
                if (carBuild.Count != 0)
                {
                    CycleIndex %= 3;
                    uIManager.CarBuildSelection(CycleIndex);
                    ToggleBuildIcon(CycleIndex);
                    buildIndex.text = (CycleIndex + 1).ToString();

                    //Need to update the current ParamsObj with the data from the loot piece to have it reflected in the menu stats bar
                    //when switching items need to reset back the data that was touched

                    if (VisualStatsManager != null)
                    {
                        currentSelectBuild = lootManager.CarPhysicsParams[CycleIndex];
                        VisualStatsManager.SetVisualStats(lootManager.CarPhysicsParams[CycleIndex]);
                    }
                }
                break;
        }
    }

    private void ToggleBuildIcon(int iconIndex)
    {
        foreach (GameObject build in carBuild)
        {
            build.SetActive(false);
        }

        carBuild[iconIndex].SetActive(true);
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
                selectionIndex = 3;
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
