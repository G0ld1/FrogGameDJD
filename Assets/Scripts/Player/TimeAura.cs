using UnityEngine;

public class TimeAura : MonoBehaviour
{
[Header("Configurações")]
    public KeyCode auraKey = KeyCode.E;
    public GameObject auraVisual; 
    public float auraRadius = 5f;
    public float slowFactor = 0.5f;

    [Header("Energia")]
    public float energy = 100f;
    public float maxEnergy = 100f;
    public float consumptionRate = 25f;
    public float recoveryRate = 15f;
    public float minEnergyToActivate = 10f;

    private bool isActive = false;
    private SphereCollider auraCollider;

    void Awake()
    {
        auraCollider = GetComponent<SphereCollider>();
        if (auraCollider == null) auraCollider = GetComponentInChildren<SphereCollider>();
        
        UpdateAuraSize();
    }

    void Update()
    {
        if (Input.GetKeyDown(auraKey))
        {
            if (!isActive && energy > minEnergyToActivate)
            {
                isActive = true;
            }
            else if (isActive)
            {
                isActive = false;
            }
        }

        if (isActive && energy <= 0)
        {
            isActive = false;
            energy = 0;
        }

        if (auraVisual != null) auraVisual.SetActive(isActive);

        if (isActive)
        {
            SfxManager.instance.PlaySFX(
                SfxManager.instance.slowdown,
                0.8f
            );
            energy -= consumptionRate * Time.deltaTime;
        }
        else if (energy < maxEnergy)
        {
            energy += recoveryRate * Time.deltaTime;
        }
        
        energy = Mathf.Clamp(energy, 0, maxEnergy);
    }

    void UpdateAuraSize()
    {
        if (auraCollider != null)
        {
            auraCollider.isTrigger = true;
            auraCollider.radius = auraRadius;
        }
        if (auraVisual != null)
        {
            float parentScale = transform.lossyScale.x;
            float finalScale = (auraRadius * 2f) / parentScale;
            auraVisual.transform.localScale = new Vector3(finalScale, finalScale, finalScale);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isActive)
        {
            StoneShard shard = other.GetComponent<StoneShard>();

            if (shard != null)
            {
                
                shard.canDamage = false; 

                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity *= Mathf.Pow(slowFactor, Time.fixedDeltaTime * 10f);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}