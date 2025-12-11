using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    public float rotationSpeed = 10f;  // Velocidade de rotação do céu (em graus por segundo)

    void Update()
    {
        // Aumenta a rotação do cubemap ao longo do tempo
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
