using UnityEngine;

public class ApplicationBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeApplication()
    {
        Application.targetFrameRate = GameConstants.TARGET_FPS;
        QualitySettings.vSyncCount = 0;
    }
}
