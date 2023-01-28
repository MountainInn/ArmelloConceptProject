using UnityEngine.Tilemaps;
using Zenject;

public class MainInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
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
            .AsTransient();

        Container
            .BindFactory<Character, Character.Factory>()
            .FromComponentInNewPrefabResource("Prefabs/Character")
            .AsTransient();

    }

    new public void Start()
    {
        base.Start();
    }
}
