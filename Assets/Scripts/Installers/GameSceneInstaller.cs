using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<CardController>().AsSingle();
        Container.Bind<GameModel>().AsSingle();
    }
}
