using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject startMenu, options, credits, connectionPanel, helpPanel;
    public RectTransform joinRoomPanel, console;

    private readonly float joinRoomPanelOpenX = 520f,
        joinRoomPanelClosedX = 1400f,
        consoleOpenY = 50f,
        consoleClosedY = -280f;

    private bool consoleIsMoving, joinRoomPanelIsMoving;
    private float consoleTargetY, joinRoomTargetX;
    public float movementSpeed = 200f;
    private bool backToMainMenu;
    public void QuitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (consoleIsMoving)
        {
            console.anchoredPosition = new Vector2(console.anchoredPosition.x,
                Mathf.Lerp(console.anchoredPosition.y, consoleTargetY, Time.deltaTime * movementSpeed));
            if (Mathf.Abs(console.anchoredPosition.y - consoleTargetY) < 1f)
            {
                consoleIsMoving = false;
            }
        }

        if (joinRoomPanelIsMoving)
        {
            joinRoomPanel.anchoredPosition =
                new Vector2(
                    Mathf.Lerp(joinRoomPanel.anchoredPosition.x, joinRoomTargetX, Time.deltaTime * movementSpeed),
                    joinRoomPanel.anchoredPosition.y);
            if (Mathf.Abs(joinRoomPanel.anchoredPosition.x - joinRoomTargetX) < 1f)
            {
                joinRoomPanelIsMoving = false;
            }
        }

        if (!joinRoomPanelIsMoving&&!consoleIsMoving&&backToMainMenu)
        {
            backToMainMenu = false;
            ToggleConsole(false);
            ToggleJoinRoomPanel(false);
            startMenu.SetActive(true);
            options.SetActive(false);
            credits.SetActive(false);
        }
        
    }

    void ToggleConsole(bool show)
    {
        consoleTargetY = show ? consoleOpenY : consoleClosedY;
        if (Mathf.Abs(console.anchoredPosition.y - consoleTargetY) > 1f)
        {
            consoleIsMoving = true;
        }
    }

    void ToggleJoinRoomPanel(bool show)
    {
        joinRoomTargetX = show ? joinRoomPanelOpenX : joinRoomPanelClosedX;
        if (Mathf.Abs(joinRoomPanel.anchoredPosition.x - joinRoomTargetX) > 1f)
        {
            joinRoomPanelIsMoving = true;
        }
    }

    public void OpenJoinGamePanel()
    {
        ToggleJoinRoomPanel(true);
    }

    public void OpenHelpPanel()
    {
        helpPanel.SetActive(true);
    }
    
    public void CloseHelpPanel()
    {
        helpPanel.SetActive(false);
    }

    public void CloseJoinGamePanel()
    {
        ToggleJoinRoomPanel(false);
    }

    public void OpenConsole()
    {
        ToggleConsole(true);
    }

    public void CloseConsole()
    {
        ToggleConsole(false);
    }

    public void BackToMainMenu()
    {
        backToMainMenu = true;
    }

    public void Play()
    {
        startMenu.SetActive(false);
        options.SetActive(false);
        credits.SetActive(false);
        connectionPanel.SetActive(true);
    }

    public void ShowOptions()
    {
        options.SetActive(true);
    }

    public void HideOptions()
    {
        options.SetActive(false);
    }

    public void ShowCredits()
    {
        credits.SetActive(true);
    }

    public void HideCredits()
    {
        credits.SetActive(false);
    }
}