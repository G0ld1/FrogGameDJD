using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public  PlayerManager playerManager;
    private bool isActivated = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
       
            
            if (playerManager != null)
            {
                playerManager.SetCheckpoint(transform.position);
                Debug.Log("Checkpoint ATIVO: " + gameObject.name);
            }
        }
    }
}
