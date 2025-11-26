using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public int targetFPS = 60; 

  
    public int vsyncMode = 0; 
    
    void Awake()
    {
       
        QualitySettings.vSyncCount = vsyncMode; 

       
        if (vsyncMode == 0)
        {
            Application.targetFrameRate = targetFPS;
            
     
            Debug.Log($"FPS Cap definido para: {targetFPS}"); 
        }
        else
        {
            Debug.Log($"Vsync está LIGADO. Target FPS ({targetFPS}) IGNORADO.");
        }
    }
}
