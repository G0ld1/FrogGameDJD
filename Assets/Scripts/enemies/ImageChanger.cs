using UnityEngine;
using UnityEngine.UI;

public class SliderTextureChanger : MonoBehaviour
{
    public Slider volumeSlider;         // Referência ao Slider
    public Renderer objectRenderer;     // O objeto 3D (por exemplo, um cubo ou esfera) que terá a textura alterada
    public Texture novolTexture;        // Textura para o estado "novol"
    public Texture lowvolTexture;       // Textura para o estado "lowvol"
    public Texture midvolTexture;       // Textura para o estado "midvol"
    public Texture maxvolTexture;       // Textura para o estado "maxvol"

    void Start()
    {
        // Inicializa o slider com um valor padrão (exemplo: 50%)
        volumeSlider.value = 0.5f;

        // Adiciona um ouvinte para quando o slider for alterado
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Inicializa a textura com base no valor inicial do slider
        UpdateTexture(volumeSlider.value);
    }

    void OnSliderValueChanged(float value)
    {
        // Altera a textura com base no valor do slider
        UpdateTexture(value);
    }

    void UpdateTexture(float value)
    {
        // Dependendo do valor do slider, altere a textura do objeto
        if (value <= 0.25f)
        {
            objectRenderer.material.mainTexture = novolTexture;
        }
        else if (value <= 0.5f)
        {
            objectRenderer.material.mainTexture = lowvolTexture;
        }
        else if (value <= 0.75f)
        {
            objectRenderer.material.mainTexture = midvolTexture;
        }
        else
        {
            objectRenderer.material.mainTexture = maxvolTexture;
        }
    }
}
