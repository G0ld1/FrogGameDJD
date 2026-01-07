using UnityEngine;

public class FinalSceneController : MonoBehaviour
{
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
    
    public void Restart()
    {
        // Recarrega a cena atual
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
