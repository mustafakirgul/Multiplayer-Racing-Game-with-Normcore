using Normal.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image boostRadialLoader;
    public Image playerHealthRadialLoader;
    public TextMeshProUGUI speedometer, playerName, timer;
    private GameObject uIPanel;
    Realtime _realtime;
    int _m, _s;
    string _mS, _sS;
    float _time;
    [HideInInspector]
    public int _bombs, _resets;
    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        uIPanel = transform.GetChild(0).gameObject;
        uIPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Analytics.CustomEvent("LEFTGAME", new Dictionary<string, object>
        {
            { "name", playerName},
            { "id", _realtime.room.clientID },
            {"time",System.DateTime.Now },
            {"bombs", _bombs},
            {"resets", _resets},
        });
            Application.Quit();
        }
        if (_realtime.room == null)
            return;
        if (_realtime.room.connected)
            _time = Time.time;
        {
            _m = Mathf.RoundToInt(_time / 60);
            _s = Mathf.RoundToInt(_time % 60);

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
