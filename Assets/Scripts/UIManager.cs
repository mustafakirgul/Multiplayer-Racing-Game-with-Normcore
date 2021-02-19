using Normal.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using JetBrains.Annotations;


public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public Image CrossHairUI;
    public RectTransform ScreenCanvas;
    public TextMeshProUGUI speedometer, playerName, timeRemaining;
    public RectTransform uIMessagePanel;
    public Text uIMessageTextBox;
    public int maximumNumberOfMessageLines = 10;
    private List<string> messages;
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

    public RawImage CurrentWeaponProjectile;
    public TextMeshProUGUI CurrentWeaponAmmoCount;

    //May be extend to include perk selection
    public TextMeshProUGUI ItemDescription;

    public CanvasGroup damageIndicatorCanvasGroup;

    //IronHog Health UI
    public GameObject IronHogHPBar;
    public float _tempTruckHealth = 0f;
    public float _lastTruckHealth = 0f;
    Coroutine InitTruckHealthCR, UpdateTruckHealthCR;

    public Camera UIcamera;

    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        _gameManager = FindObjectOfType<GameManager>();
        _lootManager = FindObjectOfType<LootManager>();
        CrossHairUI.gameObject.SetActive(false);
        uIPanel = transform.GetChild(0).gameObject;
        EnableUI();
    }

    private void Start()
    {
        messages = new List<string>();
        transform.SetParent(null);
        if (damageIndicatorCanvasGroup != null)
            damageIndicatorCanvasGroup.alpha = 0f;
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

    public void SwitchProjectileDisplayInfo(Texture2D spriteToChange, int currentAmmoCount)
    {
        CurrentWeaponProjectile.texture = spriteToChange;
        CurrentWeaponAmmoCount.text = currentAmmoCount.ToString();
    }

    public void UpdateAmmoCount(int currentAmmoCount)
    {
        CurrentWeaponAmmoCount.text = currentAmmoCount.ToString();
    }

    //Class selection button should not start here
    public void ConnectToRoom()
    {
        //Debug.LogWarning("Connecting to room.");
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

        if (_gameManager.lootTruck != null)
        {
            if (!Mathf.Approximately(_lastTruckHealth, _gameManager.lootTruck._health))
            {
                _tempTruckHealth = _lastTruckHealth;
                _lastTruckHealth = _gameManager.lootTruck._health;
            }
        }
    }

    public void InitTruckHealth()
    {
        InitTruckHealthCR = StartCoroutine(InitializeTruckHealthBar());
    }

    public void DeactivateTruckHealthUI()
    {
        if (InitTruckHealthCR != null)
            StopCoroutine(InitTruckHealthCR);
        if (UpdateTruckHealthCR != null)
            StopCoroutine(UpdateTruckHealthCR);
        IronHogHPBar.gameObject.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
        IronHogHPBar.SetActive(false);
    }

    private IEnumerator InitializeTruckHealthBar()
    {
        IronHogHPBar.SetActive(true);
        Image HealthBar = IronHogHPBar.gameObject.transform.GetChild(0).GetComponent<Image>();

        //while (HealthBar.fillAmount < 0.99f)
        {
            //_tempTruckHealth = Mathf.Lerp(_tempTruckHealth, _gameManager.lootTruck._maxHealth, Time.deltaTime * 1f);
            HealthBar.fillAmount = _tempTruckHealth / _gameManager.lootTruck._maxHealth;
            _lastTruckHealth = _gameManager.lootTruck._maxHealth;
            UpdateTruckHealthCR = StartCoroutine(UpdateTruckHealthUI());
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator UpdateTruckHealthUI()
    {
        Image HealthBar = IronHogHPBar.gameObject.transform.GetChild(0).GetComponent<Image>();

        while (true)
        {
            _tempTruckHealth = Mathf.Lerp(_tempTruckHealth, _lastTruckHealth, Time.deltaTime * 5f);
            HealthBar.fillAmount = _tempTruckHealth / _gameManager.lootTruck._maxHealth;
            yield return new WaitForEndOfFrame();
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
                    //Toggle New flags for new weapons
                    EnabledTagIfNew(i, WeaponButtonToAssign);
                    break;
                case ItemType.Armour:
                    GameObject ArmourButtonToAssign =
                        Instantiate(ArmourUIButton, ArmourGarageSlotContainer.transform);
                    ArmourButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    //Toggle New flags for new weapons
                    EnabledTagIfNew(i, ArmourButtonToAssign);
                    break;
                case ItemType.Engine:
                    GameObject EngineButtonToAssign =
                        Instantiate(EngineUIButton, EngineGarageSlotContainer.transform);
                    EngineButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    //Toggle New flags for new weapons
                    EnabledTagIfNew(i, EngineButtonToAssign);
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

    public void EnabledTagIfNew(int FlagToEnable, GameObject ButtonToEnable)
    {
        //if item is new
        if (_lootManager.playerLootPoolSave.PlayerNewLabelLootFlags[FlagToEnable])
        {
            ButtonToEnable.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            ButtonToEnable.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void AssignAdditionalLootFromGameToDisplay()
    {
        for (int i = 0; i < _lootManager.playerLootPoolSave.PlayerLootToAdd.Count; i++)
        {
            bool isNew = true;
            _lootManager.playerLootPoolSave.PlayerNewLabelLootFlags.Add(isNew);

            switch (_lootManager.playerLootPoolSave.PlayerLootToAdd[i]._ItemType)
            {
                case ItemType.Weapon:
                    GameObject WeaponButtonToAssign =
                        Instantiate(WeaponUIButton, WeaponGarageSlotContainer.transform);
                    WeaponButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    //Set the new flag to true

                    WeaponButtonToAssign.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case ItemType.Armour:
                    GameObject ArmourButtonToAssign =
                        Instantiate(ArmourUIButton, ArmourGarageSlotContainer.transform);
                    ArmourButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    //Set the new flag to true
                    ArmourButtonToAssign.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case ItemType.Engine:
                    GameObject EngineButtonToAssign =
                        Instantiate(EngineUIButton, EngineGarageSlotContainer.transform);
                    EngineButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = i;
                    //Set the new flag to true
                    EngineButtonToAssign.transform.GetChild(1).gameObject.SetActive(true);
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

    public void DisplayUIMessage(string message)
    {
        messages.Add(message);
        while (messages.Count > maximumNumberOfMessageLines)
        {
            messages.Remove(messages[0]);
        }

        uIMessageTextBox.text = "";
        for (int i = 0; i < messages.Count; i++)
        {
            uIMessageTextBox.text += (messages[i] + "\n");
        }
    }
}