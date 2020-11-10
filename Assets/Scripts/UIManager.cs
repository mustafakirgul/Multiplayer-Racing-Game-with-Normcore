using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public TextMeshProUGUI speedometer, playerName, timer;
    private GameObject uIPanel;
    int _m, _s;
    private void Start()
    {
        uIPanel = transform.GetChild(0).gameObject;
        uIPanel.SetActive(false);
    }

    private void Update()
    {
        _m = Mathf.RoundToInt(Time.time / 60);
        _s = Mathf.RoundToInt(Time.time % 60);
        timer.SetText(_m + ":" + _s);
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
