using UnityEngine;
using UnityEngine.SceneManagement;  // Necess�rio para carregar cenas
using UnityEngine.UI;              // Para acessar os componentes UI (Bot�es)

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuUI;  // Painel do menu de pausa (o Canvas ou o painel de pausa)
   
    public Slider volumeSlider;     // Se quiser controlar o volume no menu de pausa

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f) // O jogo n�o est� pausado
                Pause();
            else
                Resume();
        }
    }

    // Fun��o chamada para pausar o jogo
    public void Pause()
    {
        Debug.Log("Jogo Pausado"); // Adicionei um log de depura��o
        pauseMenuUI.SetActive(true); // Ativa o painel do menu de pausa
        Time.timeScale = 0f;         // Pausa o tempo do jogo (os objetos n�o se movem)
        Cursor.lockState = CursorLockMode.None; // Libera o cursor
        Cursor.visible = true;            // Torna o cursor vis�vel
    }

    // Fun��o chamada para retomar o jogo
    public void Resume()
    {
        Debug.Log("Jogo Retomado"); // Adicionei um log de depura��o
        pauseMenuUI.SetActive(false);  // Desativa o painel de pausa
        Time.timeScale = 1f;           // Retorna o tempo do jogo ao normal
        Cursor.lockState = CursorLockMode.Locked; // Trava o cursor
        Cursor.visible = false;           // Torna o cursor invis�vel
    }

    // Fun��o chamada para voltar ao Menu Principal
    public void LoadMainMenu()
    {
        Debug.Log("Carregando Menu Principal..."); // Log de depura��o
        Time.timeScale = 1f;           // Garante que o tempo volta ao normal
        SceneManager.LoadScene("DemoScene"); // Carrega a cena do Menu Principal
    }

    // Fun��o chamada para sair do jogo
    public void QuitGame()
    {
        // Se o jogo estiver sendo executado no Editor, para a execu��o
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    // Fecha o jogo
                    Application.Quit();
        #endif
    }
}
