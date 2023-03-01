using Mirror;
using System.Linq;
using MountainInn;

public class Aura : NetworkBehaviour
{
    readonly SyncList<Subtype> auras = new SyncList<Subtype>();

    public void Trigger(Character character)
    {
        auras
            .ToList()
            .ForEach(subtype =>
            {
                switch (subtype)
                {
                    case Subtype.Oasis:
                        character.utilityStats.health++;
                        break;

                    case Subtype.Bog:
                        break;
                       
                    default:
                        break;
                }
            });
    }

    public enum Subtype
        {
            Oasis, Bog
        }
}
