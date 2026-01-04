using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    private Vector3 currentSpawnPoint;

    private Rigidbody rb;
    
    private CameraFollow cameraScript;
    
    private PlayerMovement playerScript;
    private bool isDeadOrRespawning = false;
    private bool isInvulnerable = false;
    private float invulnerabilityDuration = 1f;
    
    private Collider[] playerColliders;
    
    public bool IsDeadOrRespawning() => isDeadOrRespawning;
    public bool IsInvulnerable() => isInvulnerable;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpawnPoint = transform.position;
        playerScript = GetComponent<PlayerMovement>();
        cameraScript = FindFirstObjectByType<CameraFollow>();
        playerColliders = GetComponents<Collider>();
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
        
        // 1. Notifica o GameManager IMEDIATAMENTE
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerLostLife();
        }
        
        // 2. Desativa colisores IMEDIATAMENTE para evitar mais dano
        foreach (Collider col in playerColliders)
        {
            col.enabled = false;
        }
        
        // 3. Desativa o movimento imediatamente
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }

          SfxManager.instance.PlaySFX(
                SfxManager.instance.damage,
                0.8f
            );
        
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log("Morreu! A tentar respawn...");
        
        if (GameManager.Instance != null && GameManager.Instance.currentLives > 0) 
        {
            Invoke("Respawn", 1f); 
        }
    }

    private void Respawn()
    {
        // 1. Move o player ao checkpoint PRIMEIRO
        Debug.Log($"Movendo player de {transform.position} para checkpoint {currentSpawnPoint}");
        transform.position = currentSpawnPoint;
        Debug.Log($"Posição após movimento: {transform.position}");
        
        // 2. Garante que o Rigidbody está desativado durante o reset
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // 3. Ativa invulnerabilidade ANTES de reativar colisores
        isInvulnerable = true;
        
        // 4. Reativa colisores
        foreach (Collider col in playerColliders)
        {
            col.enabled = true;
        }
        
        // 5. Reset do estado do player
        if (playerScript != null)
        {
            playerScript.ResetStateForRespawn();
            playerScript.enabled = true;
        }
        
        // 6. Reset da câmera
        if (cameraScript != null)
        {
            Debug.Log("A resetar câmera...");
            cameraScript.ResetCameraState();
        }
        
        Debug.Log($"Posição antes de isDeadOrRespawning = false: {transform.position}");
        isDeadOrRespawning = false;
        Debug.Log($"Posição após isDeadOrRespawning = false: {transform.position}");
        
        // 7. APENAS AGORA reativa o Rigidbody após um frame
        StartCoroutine(ReactivateRigidbodyAfterFrame());
        
        // 8. Remove invulnerabilidade após delay
        Invoke("RemoveInvulnerability", invulnerabilityDuration);
    }
    
    private System.Collections.IEnumerator ReactivateRigidbodyAfterFrame()
    {
        yield return new WaitForFixedUpdate();
        rb.isKinematic = false;
        Debug.Log($"Rigidbody reativado em posição: {transform.position}");
    }
    
    private void RemoveInvulnerability()
    {
        isInvulnerable = false;
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
