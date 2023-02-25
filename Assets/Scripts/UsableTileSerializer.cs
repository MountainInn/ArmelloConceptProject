using Mirror;

static public class UsableTileSerializer
{
    const byte
        NO_SUBTYPE=0;
    const byte
        MINING=1;

    const byte
        INFLUENCE=2;
    const byte
        INF_RESOURCE=1;

    const byte
        TRIGGER=3;
    const byte
        TRIGGER_CHARACTER_HEALTH=1;

    public static void WriteUsableTile(this NetworkWriter writer, UsableTile usableTile)
    {
        writer.WriteNetworkBehaviour(usableTile.hexTile);

        if (usableTile is MiningTile miningTile)
        {
            writer.WriteByte(MINING);
            writer.WriteInt((int)miningTile.resourceType);
        }
        else if (usableTile is InfluenceTile influenceTile)
        {
            writer.WriteByte(INFLUENCE);
            writer.WriteNetworkBehaviour(influenceTile.owner);

            if (influenceTile is InfluenceTileResource resTile)
            {
                writer.WriteByte(INF_RESOURCE);
                writer.WriteInt((int)resTile.miningTile.resourceType);
            }
        }
        else if (usableTile is TriggerTile triggerTile)
        {
            writer.WriteByte(TRIGGER);

            if (triggerTile is TriggerTileCharacterHealth charHealth)
            {
                writer.WriteByte(TRIGGER_CHARACTER_HEALTH);
            }
        }
    }

    public static UsableTile ReadUsableTile(this NetworkReader reader)
    {
        HexTile hexTile = reader.ReadNetworkBehaviour<HexTile>();
        byte type = reader.ReadByte();

        switch (type)
        {
            case MINING:
                ResourceType resourceType = (ResourceType)reader.ReadInt();
                return new MiningTile(hexTile, resourceType);

            case INFLUENCE:
                Player owner = reader.ReadNetworkBehaviour<Player>();
                InfluenceTile inflTile =
                    (reader.ReadByte()) switch
                    {
                        (INF_RESOURCE) => new InfluenceTileResource(hexTile, (ResourceType)reader.ReadInt()),
                        (_) =>
                        throw new System.Exception($"Invalid InfluenceTile subtype")
                    };

                inflTile.owner = owner;
                return inflTile;

            case TRIGGER:
                return (reader.ReadByte()) switch
                {
                    (TRIGGER_CHARACTER_HEALTH) => new TriggerTileCharacterHealth(hexTile),
                    (_) =>
                    throw new System.Exception($"Invalid TriggerTile subtype")
                };

            default:
                throw new System.Exception($"Invalid UsableTile supertype");
        };
    }
}
