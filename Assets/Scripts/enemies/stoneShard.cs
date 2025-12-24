using UnityEngine;

public class StoneShard : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 6f;
    public bool canDamage = true;

    void Start() {
        // Voa na direção para onde foi instanciado
        GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime); // Limpa o mapa para não pesar
    }

    void OnTriggerEnter(Collider other) {
        
        if (other.isTrigger) return;
        if (other.CompareTag("Player")  && canDamage) 
        {
            PlayerManager pm = other.GetComponent<PlayerManager>();
            PlayerMovement plm = other.GetComponent<PlayerMovement>();
            if (plm != null && plm.IsChosingDir) return;
            
            if (pm != null && !pm.IsDeadOrRespawning()) 
            {
                pm.Die();
                Debug.Log("Shard matou o jogador!");
                Destroy(gameObject); // Destrói a shard após o impacto
            }
        }
      
    }
}