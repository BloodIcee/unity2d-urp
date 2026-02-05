using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameConfigInstaller", menuName = "Installers/GameConfigInstaller")]
public class GameConfigInstaller : ScriptableObjectInstaller<GameConfigInstaller>
{
    public GameConfig gameConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(gameConfig).AsSingle();
    }
}
