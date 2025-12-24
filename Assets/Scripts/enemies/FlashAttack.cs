using System.Collections;
using UnityEngine;

public class FlashAttack : MonoBehaviour, IAttackBehavior
{
    [Header("Configurações do Flash")]
    public float flashRadius = 2.5f;
    public float flashIntensity = 10f;
    public float timeToExplode = 0.8f;

    [Header("Referências de Modelos")]
    public GameObject modelOff; 
    public GameObject modelOn;

    [Header("Luz")]
    public Light fireflyLight;

    public PlayerManager playerManager;
    private bool isCurrentlyOn = false;
    private bool hasBeenActivated = false; 
    private bool isWarning = false;
    private Color defaultColor;

    void Start()
    {
        if (fireflyLight != null)
        {
            defaultColor = fireflyLight.color; 
        }
        SetFlashState(false);
    }

    public void Attack(Transform enemy, Transform player)
    {
        if (hasBeenActivated || isWarning) return;

        float dist = Vector3.Distance(enemy.position, player.position);

        if (dist <= flashRadius)
        {
            StartCoroutine(ExplosionSequence(player));
            
        }
       
    }
    
    IEnumerator ExplosionSequence(Transform player)
    {
        isWarning = true;

     
        float timer = 0;
        while (timer < timeToExplode)
        {
            timer += Time.deltaTime;
          
            if (fireflyLight != null)
            {
                fireflyLight.enabled = !fireflyLight.enabled;
                fireflyLight.color = Color.red; 
                fireflyLight.intensity = 5f;
            }
            yield return new WaitForSeconds(0.05f);
        }

        // --- A EXPLOSÃO REAL ---
        hasBeenActivated = true;
        isWarning = false;
        if (fireflyLight != null) fireflyLight.color = defaultColor;
        SetFlashState(true);

        // Verifica a distância FINAL após o tempo de aviso
        float finalDist = Vector3.Distance(transform.position, player.position);
        if (finalDist <= flashRadius)
        {
            if (playerManager == null) playerManager = player.GetComponent<PlayerManager>();
            Debug.Log("Player devia levar dano");
            playerManager?.Die();
        }
        
     
        yield return new WaitForSeconds(0.5f);
        if (fireflyLight != null)
        {
            fireflyLight.enabled = true;
            fireflyLight.color = defaultColor;
            fireflyLight.intensity = flashIntensity;
            
        } // Fica ligado mas estável
    
    }

    private void SetFlashState(bool isOn)
    {
        isCurrentlyOn = isOn;

        if (modelOff != null) modelOff.SetActive(!isOn);
        if (modelOn != null) modelOn.SetActive(isOn);

        if (fireflyLight != null)
        {
            fireflyLight.enabled = isOn;
            fireflyLight.intensity = isOn ? flashIntensity : 0;
        }
    }
}