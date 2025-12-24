using UnityEngine;
using UnityEngine.SceneManagement;  // Necess�rio para carregar cenas
using UnityEngine.UI;              // Para acessar os componentes UI (Bot�es)

public class MenuController : MonoBehaviour
{
    // Fun��o chamada ao clicar no bot�o "Play"
    public void PlayGame()
    {
        // Carrega a cena "SampleScene"
        SceneManager.LoadScene("Nivel1");
    }

    // Fun��o chamada ao clicar no bot�o "Quit"
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

