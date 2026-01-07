using UnityEngine;
using UnityEngine.UI; // Necessário para interagir com a UI
using UnityEngine.SceneManagement; // Necessário para recarregar a cena
using System.Linq; // para ordenar pelos siblings

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton para fácil acesso
    
    [Header("Configuração de Vidas")]
    [SerializeField] private int maxLives = 5;
    public int currentLives;

    [Header("Configuração de UI")]
    // Array para armazenar as imagens dos corações na UI
    public Image[] heartImages; 
    public Sprite fullHeartSprite; // Sprite do coração vermelho (cheio)
    public Sprite emptyHeartSprite; // Sprite do coração preto (vazio)

    [Header("Referências")]
    public PlayerManager playerManager; // Referência ao PlayerManager

    [Header("Ligação dinâmica na cena")]
    [SerializeField] private string heartContainerTag = "UIHearts"; // defina esta tag no parent dos corações
    [SerializeField] private string heartContainerName = "Hearts";  // fallback por nome do GameObject

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        currentLives = maxLives;
        BindPlayerManager();
        BindHeartImages();
        UpdateHeartUI();

        if (heartImages != null && heartImages.Length != maxLives)
        {
            Debug.LogWarning("O número de imagens de coração na UI não corresponde ao número máximo de vidas.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindPlayerManager();
        BindHeartImages();
        UpdateHeartUI();
    }

    private void BindPlayerManager()
    {
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    private void BindHeartImages()
    {
        Image[] found = null;

        // Tenta por tag
        if (!string.IsNullOrEmpty(heartContainerTag))
        {
            var tagged = GameObject.FindWithTag(heartContainerTag);
            if (tagged != null)
                found = tagged.GetComponentsInChildren<Image>(true);
        }

        // Fallback por nome
        if ((found == null || found.Length == 0) && !string.IsNullOrEmpty(heartContainerName))
        {
            var named = GameObject.Find(heartContainerName);
            if (named != null)
                found = named.GetComponentsInChildren<Image>(true);
        }

        // Se ainda não encontrou, tenta procurar um Canvas e apanhar apenas imagens chamadas "Heart"
        if (found == null || found.Length == 0)
        {
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                var imgs = c.GetComponentsInChildren<Image>(true)
                            .Where(i => i.name.Contains("Heart"))
                            .ToArray();
                if (imgs.Length > 0)
                {
                    found = imgs;
                    break;
                }
            }
        }

        if (found != null && found.Length > 0)
        {
            // Ordena pela posição na hierarquia para manter a ordem visual
            heartImages = found.OrderBy(i => i.transform.GetSiblingIndex()).ToArray();
        }
        else
        {
            heartImages = System.Array.Empty<Image>();
            Debug.LogWarning("Não foram encontradas imagens de corações na cena atual.");
        }
    }

    public void PlayerLostLife()
    {
        currentLives--;
        Debug.Log("Vida perdida. Vidas restantes: " + currentLives);
        UpdateHeartUI();

        if (currentLives <= 0)
        {
            Invoke("GameOver", 2f); 
        }
    }

    private void UpdateHeartUI()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentLives)
                heartImages[i].sprite = fullHeartSprite;
            else
                heartImages[i].sprite = emptyHeartSprite;
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER! A recarregar a cena...");
        currentLives = maxLives;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}