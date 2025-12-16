using UnityEngine;
using System.Collections;

public class EnemyFormSwitcher : MonoBehaviour
{
    public float switchRange = 5f;
    public float knockbackRange = 6f;


    public GameObject normalForm;
    public GameObject alertForm;

    [Header("Flashbang")]
    public CanvasGroup flashCanvas; // Canvas branco fullscreen
    public float flashDuration = 10f;

    private Transform player;
    private bool isAlert = false;
    private bool hasBeenAlerted = false; // nunca mais volta ao A

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;

        normalForm.SetActive(true);
        alertForm.SetActive(false);

        if (flashCanvas != null)
            flashCanvas.alpha = 0f;
    }

    void Update()
    {
        if (player == null || hasBeenAlerted) return;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;
        float dist = Vector2.Distance(enemyPos, playerPos);

        if (dist <= switchRange && !isAlert)
        {
            ActivateAlertMode();
        }
    }

    void ActivateAlertMode()
    {
        isAlert = true;
        hasBeenAlerted = true;

        normalForm.SetActive(false);
        alertForm.SetActive(true);

        if (flashCanvas != null)
            StartCoroutine(Flashbang());

        PlayerKnockback knockback = player.GetComponent<PlayerKnockback>();

        if (knockback != null)
        {
            knockback.ApplyKnockback(transform.position);
        }


        Debug.Log("ALERT MODE ON (LOCKED)");
    }


    IEnumerator Flashbang()
    {
        float t = 0f;
        float fadeInTime = 0.05f;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            flashCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeInTime);
            yield return null;
        }

        flashCanvas.alpha = 1f;

        yield return new WaitForSeconds(0.4f);

        t = 0f;
        float fadeOutTime = 0.45f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            flashCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }

        flashCanvas.alpha = 0f;
    }


}
