using UnityEngine;
using UnityEngine.UI; // Necessário para interagir com a UI
using UnityEngine.SceneManagement; // Necessário para recarregar a cena

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
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           DontDestroyOnLoad(gameObject);
        }
      
    }

    void Start()
    {
        currentLives = maxLives;
        UpdateHeartUI();
        
        // Garante que o PlayerManager está ligado
        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }
        
        // Uma verificação de segurança importante
        if (heartImages.Length != maxLives)
        {
            Debug.LogError("O número de imagens de coração na UI não corresponde ao número máximo de vidas!");
        }
    }

    public void PlayerLostLife()
    {
        currentLives--;
        Debug.Log("Vida perdida. Vidas restantes: " + currentLives);
        
        UpdateHeartUI(); // Atualiza a UI imediatamente

        if (currentLives <= 0)
        {
            Invoke("GameOver", 2f); 
        }
        
    }

    private void UpdateHeartUI()
    {
        for (int i = 0; i < maxLives; i++)
        {
            if (i < currentLives)
            {
                // Este coração está cheio (vermelho)
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                // Este coração está vazio (preto)
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER! A recarregar a cena...");
        
        // Recarrega a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}