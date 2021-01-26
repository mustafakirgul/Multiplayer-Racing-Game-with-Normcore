using Normal.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public TextMeshProUGUI speedometer, playerName, timeRemaining;
    private GameObject uIPanel;
    public GameObject enterNamePanel;

    //UI Slots for loot inventory
    //Assigned manually
    public AutoResizeLootRectTransforms[] AutoResizeLootRectTransforms;

    //References to important things
    Realtime _realtime;
    GameManager _gameManager;
    LootManager _lootManager;

    //Assigned manually 
    //This is a reference to the UI container
    public GameObject WeaponGarageSlotContainer;
    public GameObject ArmourGarageSlotContainer;
    public GameObject EngineGarageSlotContainer;

    //Assigned manually as a reference to populate the UI
    public GameObject WeaponUIButton;
    public GameObject ArmourUIButton;
    public GameObject EngineUIButton;

    private int lastbuildSelected;

    public int SelectedBuildToView;

    //Need to add details the mesh of each car
    public List<GameObject> BuildModelsAppearance = new List<GameObject>();

    //UI weapon display for current build
    public RawImage SelectedWeapon;
    public RawImage SelectedEngine;
    public RawImage SelectedArmour;

    //May be extend to include perk selection
    public TextMeshProUGUI ItemDescription;

    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        _gameManager = FindObjectOfType<GameManager>();
        _lootManager = FindObjectOfType<LootManager>();
        uIPanel = transform.GetChild(0).gameObject;
        EnableUI();
    }

    private void Start()
    {
        _gameManager.FixAssociations();
        AssignLootToDisplayAtStart();
        AssignLoadOutLootItemVisualImage(_lootManager.selected_buildLoadOutToView);
    }

    //Need to create logic here to select specific models to showcase
    //Based on the selection of each model
    public void CarBuildSelection(int _buildIndex)
    {
        for (int i = 0; i < BuildModelsAppearance.Count; i++)
        {
            BuildModelsAppearance[i].SetActive(false);
        }

        BuildModelsAppearance[_buildIndex].SetActive(true);

        _lootManager.RetreiveBuild(_buildIndex);
        SelectedBuildToView = _buildIndex;
        //Add loadout image visualizations
        AssignLoadOutLootItemVisualImage(_lootManager.selected_buildLoadOutToView);
    }

    //Class selection button should not start here
    public void ConnectToRoom()
    {
        for (int i = 0; i < BuildModelsAppearance.Count; i++)
        {
            BuildModelsAppearance[i].SetActive(false);
        }

        GameManager.instance.ConnectToRoom(SelectedBuildToView);
        _lootManager.DeploySelectedBuild();
        lastbuildSelected = SelectedBuildToView;
    }

    private void AssignLoadOutLootItemVisualImage(BuildLoadOutSObj build)
    {
        SelectedWeapon.texture = build.Weapon.m_image;
        SelectedArmour.texture = build.Armour.m_image;
        SelectedEngine.texture = build.Engine.m_image;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            _realtime.Disconnect();
            DisableUI();
            enterNamePanel.SetActive(true);
        }

        //To be removed in final build
        if (Input.GetKeyDown(KeyCode.N))
        {
            _lootManager.numberOfLootRolls++;
            //_lootManager.RollForLoot();
            //ResizeUILootContainers();
        }
    }

    public void ReactivateLogin()
    {
        DisableUI();
        enterNamePanel.SetActive(true);
        //Select Default build to display
        //To Do: remember the load saved build
        CarBuildSelection(lastbuildSelected);
    }

    //this needs to run first before the UI methods run
    //With sObjImplementation this will only need to be done once
    public void AssignLootToDisplayAtStart()
    {
        for (int i = 0; i < _lootManager.playerLootPoolSave.PlayerLoot.Count; i++)
        {
            switch (_lootManager.playerLootPoolSave.PlayerLoot[i]._ItemType)
            {
                case ItemType.Weapon:
                    GameObject WeaponButtonToAssign =
                        Instantiate(WeaponUIButton, WeaponGarageSlotContainer.transform);
                    WeaponButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    break;
                case ItemType.Armour:
                    GameObject ArmourButtonToAssign =
                        Instantiate(ArmourUIButton, ArmourGarageSlotContainer.transform);
                    ArmourButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    break;
                case ItemType.Engine:
                    GameObject EngineButtonToAssign =
                        Instantiate(EngineUIButton, EngineGarageSlotContainer.transform);
                    EngineButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    break;
                default:
                    break;
            }
        }

        if (AutoResizeLootRectTransforms.Length != 0)
        {
            foreach (AutoResizeLootRectTransforms containers in AutoResizeLootRectTransforms)
            {
                containers.ResizeLootScrollBar();
            }
        }
    }

    public void AssignAdditionalLootFromGameToDisplay()
    {
        for (int i = 0; i < _lootManager.playerLootPoolSave.PlayerLootToAdd.Count; i++)
        {
            switch (_lootManager.playerLootPoolSave.PlayerLootToAdd[i]._ItemType)
            {
                case ItemType.Weapon:
                    GameObject WeaponButtonToAssign =
                        Instantiate(WeaponUIButton, WeaponGarageSlotContainer.transform);
                    WeaponButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    break;
                case ItemType.Armour:
                    GameObject ArmourButtonToAssign =
                        Instantiate(ArmourUIButton, ArmourGarageSlotContainer.transform);
                    ArmourButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    break;
                case ItemType.Engine:
                    GameObject EngineButtonToAssign =
                        Instantiate(EngineUIButton, EngineGarageSlotContainer.transform);
                    EngineButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    break;
                default:
                    break;
            }
        }

        if (AutoResizeLootRectTransforms.Length != 0)
        {
            foreach (AutoResizeLootRectTransforms containers in AutoResizeLootRectTransforms)
            {
                containers.ResizeLootScrollBar();
            }
        }

        //Empty array for next round
        _lootManager.playerLootPoolSave.PlayerLootToAdd.Clear();
    }

    public void ResizeUILootContainers()
    {
        if (AutoResizeLootRectTransforms.Length != 0)
        {
            foreach (AutoResizeLootRectTransforms containers in AutoResizeLootRectTransforms)
            {
                containers.ResizeLootScrollBar();
            }
        }
    }

    public void EnableUI()
    {
        uIPanel.SetActive(true);
    }

    public void DisableUI()
    {
        uIPanel.SetActive(false);
    }
}