using UnityEngine.Tilemaps;
using Zenject;

public class MainInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<EOSLobbyUI>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container
            .Bind<Map>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container
            .Bind<Tilemap>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container
            .BindFactory<Player, Player.Factory>()
            .FromComponentInNewPrefabResource("Prefabs/PlayerPrefab")
            .AsTransient()
            .Lazy();

        Container
            .BindFactory<Character, Character.Factory>()
            .FromComponentInNewPrefabResource("Prefabs/Character")
            .AsTransient()
            .Lazy();
    }

    public DiContainer GetContainer() => Container;
}
