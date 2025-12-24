using UnityEngine;

public class Killzone : MonoBehaviour
{
    
    public PlayerManager playerManager;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           
            Debug.Log(" collider ativado pelo script de killzone");
            if (playerManager != null && !playerManager.IsDeadOrRespawning())
            {
                playerManager.Die(); 
            }
        }
    }
}
