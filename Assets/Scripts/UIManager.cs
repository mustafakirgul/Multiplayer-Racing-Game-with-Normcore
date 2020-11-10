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
    string _mS, _sS;
    private void Start()
    {
        uIPanel = transform.GetChild(0).gameObject;
        uIPanel.SetActive(false);
    }

    private void Update()
    {
        _m = Mathf.RoundToInt(Time.time / 60);
        _s = Mathf.RoundToInt(Time.time % 60);

        if (_m < 10)
            _mS = "0" + _m.ToString();
        else
            _mS = _m.ToString();

        if (_s < 10)
            _sS = "0" + _s.ToString();
        else
            _sS = _s.ToString();

        timer.SetText(_mS + ":" + _sS);
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
