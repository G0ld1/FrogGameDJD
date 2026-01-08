// csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    [Tooltip("If empty, buttons will be auto-discovered from children (hierarchy order).")]
    public List<Button> menuButtons = new List<Button>();

    public float inputRepeatDelay = 0.25f;
    private float inputTimer = 0f;

    private int currentIndex = 0;

    void Start()
    {
        if (menuButtons.Count == 0)
        {
            menuButtons.AddRange(GetComponentsInChildren<Button>(true));
        }

        if (menuButtons.Count > 0)
        {
            currentIndex = 0;
            SelectCurrent();
        }
    }

    void Update()
    {
        if (menuButtons == null || menuButtons.Count == 0) return;

        // horizontal navigation: prefer InputManager movement.x, fallback to old axis
        float nav = InputManager.movement.x;
        if (Mathf.Abs(nav) < 0.01f)
        {
            nav = Input.GetAxisRaw("Horizontal");
        }

        inputTimer -= Time.unscaledDeltaTime;

        if (nav > 0.5f && inputTimer <= 0f)
        {
            MoveSelection(1);
            inputTimer = inputRepeatDelay;
        }
        else if (nav < -0.5f && inputTimer <= 0f)
        {
            MoveSelection(-1);
            inputTimer = inputRepeatDelay;
        }

        // submit (controller button or Enter/Space)
        if (InputManager.submitWasPressed)
        {
            ActivateCurrent();
        }
    }

    private void MoveSelection(int dir)
    {
        if (menuButtons == null || menuButtons.Count == 0) return;

        currentIndex = (currentIndex + dir + menuButtons.Count) % menuButtons.Count;
        SelectCurrent();
    }

    private void SelectCurrent()
    {
        var btn = menuButtons[currentIndex];
        if (btn == null) return;

        // make sure EventSystem also knows about the selection (for visual focus)
        EventSystem.current?.SetSelectedGameObject(btn.gameObject);
        btn.Select();
    }

    private void ActivateCurrent()
    {
        if (menuButtons == null || menuButtons.Count == 0) return;

        var btn = menuButtons[currentIndex];
        if (btn != null)
        {
            btn.onClick.Invoke();
        }
    }

    public void PlayGame()
    {
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Nivel1");
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
