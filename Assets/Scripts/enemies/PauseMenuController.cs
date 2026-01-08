using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Slider volumeSlider;

    [Tooltip("Optional: populate with the buttons in the pause menu in order. If empty, buttons are autodiscovered.")]
    public List<Button> menuButtons = new List<Button>();

    private int currentIndex = 0;
    private bool isPaused = false;

    // input repeat control for stick/dpad
    public float inputRepeatDelay = 0.25f;
    private float inputTimer = 0f;

    void Start()
    {
        if (menuButtons.Count == 0 && pauseMenuUI != null)
        {
            menuButtons.AddRange(pauseMenuUI.GetComponentsInChildren<Button>(true));
        }

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // Toggle pause with Menu action or Escape
        if (InputManager.menuWasPressed)
        {
            if (!isPaused) Pause();
            else Resume();
        }

        if (!isPaused) return;

        // navigation input: prefer InputManager movement.y then fallback to old Input axis
        float nav = InputManager.movement.y;
        if (Mathf.Abs(nav) < 0.01f)
            nav = Input.GetAxisRaw("Vertical");

        inputTimer -= Time.unscaledDeltaTime;

        if (nav > 0.5f && inputTimer <= 0f)
        {
            MoveSelection(-1);
            inputTimer = inputRepeatDelay;
        }
        else if (nav < -0.5f && inputTimer <= 0f)
        {
            MoveSelection(1);
            inputTimer = inputRepeatDelay;
        }

        // submit with InputManager submit or Enter/Space/X
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
        if (btn != null)
        {
            btn.Select();
        }
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

    public void Pause()
    {
        isPaused = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // default select first
        currentIndex = 0;
        SelectCurrent();
    }

    public void Resume()
    {
        isPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadMainMenu()
    {
       
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}