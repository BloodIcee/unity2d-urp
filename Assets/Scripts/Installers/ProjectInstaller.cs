using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private GameConfig gameConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(gameConfig).AsSingle();
        
        Container.Bind<GameStateMachine>().AsSingle();
    }
}
