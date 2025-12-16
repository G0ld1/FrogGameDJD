using UnityEngine;

public class WinItem : MonoBehaviour
{
    
    public PlayerManager playerManager;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            if (playerManager != null)
            {
                playerManager.WinGame();
                
                Destroy(gameObject);
            }
        }
    }
}
