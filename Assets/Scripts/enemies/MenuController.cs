using UnityEngine;
using UnityEngine.SceneManagement;  // Necessário para carregar cenas
using UnityEngine.UI;              // Para acessar os componentes UI (Botões)

public class MenuController : MonoBehaviour
{
    // Função chamada ao clicar no botão "Play"
    public void PlayGame()
    {
        // Carrega a cena "SampleScene"
        SceneManager.LoadScene("SampleScene");
    }

    // Função chamada ao clicar no botão "Quit"
    public void QuitGame()
    {
        // Se o jogo estiver sendo executado no Editor, para a execução
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Fecha o jogo
            Application.Quit();
#endif
    }
}

