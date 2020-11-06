using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public TextMeshProUGUI speedometer, playerName;
    private GameObject uIPanel;
    private void Start()
    {
        uIPanel = transform.GetChild(0).gameObject;
        uIPanel.SetActive(false);
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
