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
    Realtime _realtime;

    public List<GameObject> BuildModels = new List<GameObject>();
    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        uIPanel = transform.GetChild(0).gameObject;
        EnableUI();
    }
    private void Start()
    {
        FindObjectOfType<GameManager>().FixAssociations();
    }

    //Need to create logic here to select specific models to showcase
    //Based on the selection of each model
    public void CarBuildSelection(int _buildIndex)
    {
        for (int i = 0; i < BuildModels.Count; i++)
        {
            BuildModels[i].SetActive(false);
        }

        BuildModels[_buildIndex].SetActive(true);
    }

    //Class selection button should not start here
    public void ConnectToRoom(int _index)
    {
        for (int i = 0; i < BuildModels.Count; i++)
        {
            BuildModels[i].SetActive(false);
        }

        GameManager.instance.ConnectToRoom(_index);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            _realtime.Disconnect();
            DisableUI();
            enterNamePanel.SetActive(true);
        }
    }
    public void ReactivateLogin()
    {
        DisableUI();
        enterNamePanel.SetActive(true);
        CarBuildSelection(1);
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
