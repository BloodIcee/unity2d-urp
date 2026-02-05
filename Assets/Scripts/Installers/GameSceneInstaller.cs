using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameStateMachine>().AsSingle();
        Container.Bind<AnimationService>().AsSingle();
        Container.Bind<IScoringService>().To<ScoringService>().AsSingle();
        Container.Bind<CardController>().AsSingle();
        Container.Bind<GameModel>().AsSingle();
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();
    }
}
