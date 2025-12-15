using UnityEngine;

public class EnemyFormSwitcher : MonoBehaviour
{
    public float switchRange = 5f;

    public GameObject normalForm;
    public GameObject alertForm;

    private Transform player;
    private bool isAlert = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        normalForm.SetActive(true);
        alertForm.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        Vector2 enemyPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPos = new Vector2(player.position.x, player.position.y);
        float dist = Vector2.Distance(enemyPos, playerPos);

        if (dist <= switchRange && !isAlert)
        {
            ActivateAlertMode();
        }
        else if (dist > switchRange && isAlert)
        {
            ActivateNormalMode();
        }
    }

    void ActivateAlertMode()
    {
        isAlert = true;
        normalForm.SetActive(false);
        alertForm.SetActive(true);
        Debug.Log("ALERT MODE ON");
    }

    void ActivateNormalMode()
    {
        isAlert = false;
        normalForm.SetActive(true);
        alertForm.SetActive(false);
        Debug.Log("ALERT MODE OFF");
    }
}
