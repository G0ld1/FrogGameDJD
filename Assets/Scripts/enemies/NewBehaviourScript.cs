using UnityEngine;
using UnityEngine.UI;

public class AudioVolumeController : MonoBehaviour
{
    public Slider volumeSlider;         // Referência ao Slider
    public AudioSource audioSource;     // Referência ao AudioSource para controlar o volume
    public Image displayImage;          // Imagem que será alterada
    public Sprite novolImage;           // Imagem para o estado "novol"
    public Sprite lowvolImage;          // Imagem para o estado "lowvol"
    public Sprite midvolImage;          // Imagem para o estado "midvol"
    public Sprite maxvolImage;          // Imagem para o estado "maxvol"

    void Start()
    {
        // Inicializa o slider com o valor atual do volume
        volumeSlider.value = audioSource.volume;

        // Adiciona o ouvinte para quando o slider for alterado
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Atualiza a imagem e o volume no início com base no valor do slider
        UpdateImageAndVolume(volumeSlider.value);
    }

    // Método chamado quando o valor do slider muda
    void OnSliderValueChanged(float value)
    {
        // Ajusta o volume e a imagem com base no valor do slider
        UpdateImageAndVolume(value);
    }

    // Atualiza tanto o volume quanto a imagem com base no valor do slider
    void UpdateImageAndVolume(float value)
    {
        // Ajusta o volume do AudioSource
        audioSource.volume = value;

        // Alterar a imagem de acordo com o valor do slider
        if (value <= 0.25f)
        {
            displayImage.sprite = novolImage; // Imagem para volume baixo
        }
        else if (value <= 0.5f)
        {
            displayImage.sprite = lowvolImage; // Imagem para volume médio-baixo
        }
        else if (value <= 0.75f)
        {
            displayImage.sprite = midvolImage; // Imagem para volume médio
        }
        else
        {
            displayImage.sprite = maxvolImage; // Imagem para volume alto
        }
    }
}
