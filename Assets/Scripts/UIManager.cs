using Normal.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public TextMeshProUGUI speedometer, playerName, timeRemaining;
    private GameObject uIPanel;
    public GameObject enterNamePanel;
    Realtime _realtime;
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

    public void ConnectToRoom(int _index)
    {
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
    public void EnableUI()
    {
        uIPanel.SetActive(true);
    }
    public void DisableUI()
    {
        uIPanel.SetActive(false);
    }
}
