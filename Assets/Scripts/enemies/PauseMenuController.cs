using UnityEngine;
using UnityEngine.SceneManagement;  // Necessário para carregar cenas
using UnityEngine.UI;              // Para acessar os componentes UI (Botões)

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuUI;  // Painel do menu de pausa (o Canvas ou o painel de pausa)
    public GameObject player;       // O jogador (se necessário para pausar o movimento ou física)
    public Slider volumeSlider;     // Se quiser controlar o volume no menu de pausa

    void Update()
    {
        // Detecta a pressão da tecla ESC para ativar/desativar o menu de pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f) // O jogo não está pausado
                Pause();
            else
                Resume();
        }
    }

    // Função chamada para pausar o jogo
    public void Pause()
    {
        Debug.Log("Jogo Pausado"); // Adicionei um log de depuração
        pauseMenuUI.SetActive(true); // Ativa o painel do menu de pausa
        Time.timeScale = 0f;         // Pausa o tempo do jogo (os objetos não se movem)
        Cursor.lockState = CursorLockMode.None; // Libera o cursor
        Cursor.visible = true;            // Torna o cursor visível
    }

    // Função chamada para retomar o jogo
    public void Resume()
    {
        Debug.Log("Jogo Retomado"); // Adicionei um log de depuração
        pauseMenuUI.SetActive(false);  // Desativa o painel de pausa
        Time.timeScale = 1f;           // Retorna o tempo do jogo ao normal
        Cursor.lockState = CursorLockMode.Locked; // Trava o cursor
        Cursor.visible = false;           // Torna o cursor invisível
    }

    // Função chamada para voltar ao Menu Principal
    public void LoadMainMenu()
    {
        Debug.Log("Carregando Menu Principal..."); // Log de depuração
        Time.timeScale = 1f;           // Garante que o tempo volta ao normal
        SceneManager.LoadScene("MainMenu"); // Carrega a cena do Menu Principal
    }

    // Função chamada para sair do jogo
    public void QuitGame()
    {
        Debug.Log("Saindo do Jogo..."); // Log de depuração
        Application.Quit();            // Fecha o jogo
    }
}
