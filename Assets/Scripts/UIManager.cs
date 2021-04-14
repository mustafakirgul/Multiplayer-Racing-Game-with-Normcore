using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public Image CrossHairUI;
    public RectTransform ScreenCanvas;
    public TextMeshProUGUI speedometer, playerName, timeRemaining;
    private GameObject uIPanel;
    public GameObject enterNamePanel;

    public Image[] buildSelectionLights;

    //UI Slots for loot inventory
    //Assigned manually
    public AutoResizeLootRectTransforms[] AutoResizeLootRectTransforms;

    //References to important things
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
    public List<ContainerSelector> VisualModelSelectors = new List<ContainerSelector>();

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

    public Image HitMarker;

    [SerializeField]
    BuildScrollSelector m_buildScrollSelector;

    [SerializeField]
    GameObject ReadyToStartMenu;

    public Image HealthBar;

    public Image OverheatMeter;
    public GameObject OverheatMeterObj;
    public GameObject OverheatNotice;
    public GameObject WeaponSwitchIcon;

    public GameObject LootTruckInvincibleIcon;

    public Image BoostPUMeter, DefensePUMeter, HandlinePUMeter, HogDmgPUMeter;

    public Text currentLootCount;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _lootManager = FindObjectOfType<LootManager>();
        CrossHairUI.gameObject.SetActive(false);
        uIPanel = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        if (damageIndicatorCanvasGroup != null)
            damageIndicatorCanvasGroup.alpha = 0f;
        _gameManager.FixAssociations();
        AssignLootToDisplayAtStart();
        AssignLoadOutLootItemVisualImage(_lootManager.selected_buildLoadOutToView);
        ContainerSelector[] selector = BuildModelsAppearance[0].GetComponentsInChildren<ContainerSelector>();

        VisualModelSelectors.Clear();
        foreach (ContainerSelector sel in selector)
        {
            VisualModelSelectors.Add(sel);
        }

        MakeHitMarkerDissappear();
    }

    //Need to create logic here to select specific models to showcase
    //Based on the selection of each model
    public void CarBuildSelection(int _buildIndex)
    {
        for (int i = 0; i < BuildModelsAppearance.Count; i++)
        {
            BuildModelsAppearance[i].SetActive(false);
        }

        for (int i = 0; i < buildSelectionLights.Length; i++)
        {
            buildSelectionLights[i].enabled = i == _buildIndex;
        }

        BuildModelsAppearance[_buildIndex].SetActive(true);

        ContainerSelector[] selector = BuildModelsAppearance[_buildIndex].GetComponentsInChildren<ContainerSelector>();

        VisualModelSelectors.Clear();
        foreach (ContainerSelector sel in selector)
        {
            VisualModelSelectors.Add(sel);
        }

        _lootManager.RetreiveBuild(_buildIndex);
        SelectedBuildToView = _buildIndex;
        //Add loadout image visualizations
        AssignLoadOutLootItemVisualImage(_lootManager.selected_buildLoadOutToView);
    }

    public void UpdateCarVisualModelsWeapons(int indexToActivate)
    {
        //Index 0 is the weapon
        if (indexToActivate < 7)
        {
            VisualModelSelectors[3].DeActivateChildren();
            VisualModelSelectors[0].ActivateItem(indexToActivate);
        }
        else
        {
            //Activate melee armour instead
            VisualModelSelectors[0].ActivateItem(6);
            VisualModelSelectors[3].ActivateItem(indexToActivate - 8);
        }
    }

    public void UpdateCarVisualModelsArmour(int indexToActivate)
    {
        //Index 1 is the armour
        VisualModelSelectors[1].ActivateItem(indexToActivate);
    }

    public void UpdateCarVisualModelsEngines(int indexToActivate)
    {
        //Index 2 is the Engine
        VisualModelSelectors[2].ActivateItem(indexToActivate);
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

    public void ConfirmHitDamage()
    {
        MakeHitMarkerAppear();
        StartCoroutine(FadeCrossHair());
    }

    private void MakeHitMarkerAppear()
    {
        Color fadeColor = HitMarker.color;
        fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        HitMarker.color = fadeColor;
    }

    private void MakeHitMarkerDissappear()
    {
        Color fadeColor = HitMarker.color;
        fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        HitMarker.color = fadeColor;
    }

    private IEnumerator FadeCrossHair()
    {
        while (HitMarker.color.a > 0)
        {
            Color fadeColor = HitMarker.color;
            float fadeAmt = fadeColor.a - (Time.deltaTime / 2f);

            fadeColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmt);
            HitMarker.color = fadeColor;
            yield return null;
        }
    }

    public void ConnectToRoom()
    {
        //Debug.LogWarning("Connecting to room.");
        for (int i = 0; i < BuildModelsAppearance.Count; i++)
        {
            BuildModelsAppearance[i].SetActive(false);
        }

        GameManager.instance.StartTheRace(SelectedBuildToView);
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
        if (HealthBar == null) HealthBar = IronHogHPBar.gameObject.transform.GetChild(0).GetComponent<Image>();

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
        if (HealthBar == null) HealthBar = IronHogHPBar.gameObject.transform.GetChild(0).GetComponent<Image>();

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
        LobbyManager.instance.Reset();
        enterNamePanel.SetActive(true);
        //Select Default build to display
        //To Do: remember the load saved build
        CarBuildSelection(lastbuildSelected);
        ReadyToStartMenu.SetActive(false);

        //Visual things add back
        ContainerSelector[] selector =
            BuildModelsAppearance[lastbuildSelected].GetComponentsInChildren<ContainerSelector>();

        VisualModelSelectors.Clear();
        foreach (ContainerSelector sel in selector)
        {
            VisualModelSelectors.Add(sel);
        }
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
        int lootIndexToStartAt = _lootManager.playerLootPoolSave.PlayerLoot.Count;
        for (int i = 0; i < _lootManager.playerLootPoolSave.PlayerLootToAdd.Count; i++)
        {
            bool isNew = true;
            _lootManager.playerLootPoolSave.PlayerNewLabelLootFlags.Add(isNew);
            _lootManager.playerLootPoolSave.PlayerLoot.Add(_lootManager.playerLootPoolSave.PlayerLootToAdd[i]);

            switch (_lootManager.playerLootPoolSave.PlayerLootToAdd[i]._ItemType)
            {
                case ItemType.Weapon:
                    GameObject WeaponButtonToAssign =
                        Instantiate(WeaponUIButton, WeaponGarageSlotContainer.transform);
                    WeaponButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = (i + lootIndexToStartAt);
                    //Set the new flag to true

                    WeaponButtonToAssign.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case ItemType.Armour:
                    GameObject ArmourButtonToAssign =
                        Instantiate(ArmourUIButton, ArmourGarageSlotContainer.transform);
                    ArmourButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = (i + lootIndexToStartAt);
                    //Set the new flag to true
                    ArmourButtonToAssign.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case ItemType.Engine:
                    GameObject EngineButtonToAssign =
                        Instantiate(EngineUIButton, EngineGarageSlotContainer.transform);
                    EngineButtonToAssign.GetComponent<UIItemDataContainer>()._buttonItemID = (i + lootIndexToStartAt);
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

    public void ResetLootUI()
    {
        if (m_buildScrollSelector != null)
            m_buildScrollSelector.InitializeManualSelection();
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