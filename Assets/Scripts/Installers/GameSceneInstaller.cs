using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private AudioConfig audioConfig;

    public override void InstallBindings()
    {
        Container.BindInstance(audioConfig);

        Container.Bind<GameStateMachine>().AsSingle();
        Container.Bind<AnimationService>().AsSingle();
        Container.Bind<AudioManager>().FromNewComponentOnNewGameObject().AsSingle();
        Container.Bind<IScoringService>().To<ScoringService>().AsSingle();
        Container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle();
        Container.Bind<IGameStateManager>().To<GameStateManager>().AsSingle();
        Container.Bind<CardController>().AsSingle();
        Container.Bind<GameModel>().AsSingle();
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();
    }
}
