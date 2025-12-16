using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    private Vector3 currentSpawnPoint;

    private Rigidbody rb;
    
    private CameraFollow cameraScript;
    
    private PlayerMovement playerScript;
    private bool isDeadOrRespawning = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpawnPoint = transform.position;
        playerScript = GetComponent<PlayerMovement>();
        cameraScript = FindFirstObjectByType<CameraFollow>();
    }

    public void SetCheckpoint(Vector3 position)
    {
        currentSpawnPoint = position;
        Debug.Log("Checkpoint Ativado em: " + position);
    }

    public void Die()
    {
        if (isDeadOrRespawning) return; 

        isDeadOrRespawning = true;
        
        rb.isKinematic = true;

        Debug.Log("Morreu! A tentar respawn...");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerLostLife();
        }
        
        if (GameManager.Instance != null && GameManager.Instance.currentLives > 0) 
        {
            Invoke("Respawn", 1f); 
        }
      
    }

    private void Respawn()
    {
        
        if (cameraScript != null)
        {
            cameraScript.ResetCameraState();
        }
        
        if (playerScript != null)
        {
            playerScript.ResetStateForRespawn();
        }
        transform.position = currentSpawnPoint;
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        rb.isKinematic = false;
        isDeadOrRespawning = false;
       
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
