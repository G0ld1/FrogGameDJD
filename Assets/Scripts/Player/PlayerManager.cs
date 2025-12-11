using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    private Vector3 currentSpawnPoint;

    private Rigidbody rb;
    
    private CameraFollow cameraScript;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpawnPoint = transform.position;
        cameraScript = FindFirstObjectByType<CameraFollow>();
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentSpawnPoint = position;
        Debug.Log("Checkpoint Ativado em: " + position);
    }

    public void Die()
    {


        Debug.Log("Morreu! A tentar respawn...");
        Invoke("Respawn", 1f); 
    }

    private void Respawn()
    {
        transform.position = currentSpawnPoint;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        if (cameraScript != null)
        {
            cameraScript.ResetCameraState();
        }
    }
    
    public void WinGame()
    {
       

        
        GoToNextScene();
    }
    
    private void GoToNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Não há próxima cena. Fim do Jogo.");
        }
    }
}
