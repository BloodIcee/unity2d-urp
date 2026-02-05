using UnityEngine;

public class ApplicationBootstrap : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = GameConstants.TARGET_FPS;
        QualitySettings.vSyncCount = 0;
    }
}
