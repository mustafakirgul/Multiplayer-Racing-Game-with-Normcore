using UnityEngine;

public class Help : MonoBehaviour
{
    GameObject _panel;
    bool _state;
    private void Start()
    {
        _panel = transform.GetChild(0).gameObject;
        _panel.SetActive(_state);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            _state = !_state;
            _panel.SetActive(_state);
        }
    }
}
