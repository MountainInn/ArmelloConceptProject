using UnityEngine.Tilemaps;
using Zenject;

public class MainInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind(
                typeof(FlagPool),
                typeof(ResourcesView),
                typeof(PlayerCustomizationView),
                typeof(TurnView),
                typeof(CombatView),
                typeof(EOSLobbyUI),
                typeof(CubeMap),
                typeof(TurnSystem))
            .FromComponentInHierarchy()
            .AsSingle();

        Container
            .Bind<Character>()
            .FromComponentInNewPrefabResource("Prefabs/Character")
            .AsSingle()
            .Lazy();

        Container
            .Bind<Player>()
            .FromComponentInNewPrefabResource("Prefabs/Player")
            .AsSingle()
            .Lazy();


        Container
            .Bind<StatView>()
            .FromResource("Prefabs/Unit View")
            .AsSingle()
            .Lazy();
    }

    public DiContainer GetContainer() => Container;
}
