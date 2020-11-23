﻿using Normal.Realtime;
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
    public GameObject enterNamePanel;
    Realtime _realtime;
    int _m, _s;
    string _mS, _sS;
    float _time;

    private void Awake()
    {
        _realtime = FindObjectOfType<Realtime>();
        uIPanel = transform.GetChild(0).gameObject;
        EnableUI();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            _realtime.Disconnect();
            DisableUI();
            enterNamePanel.SetActive(true);
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
