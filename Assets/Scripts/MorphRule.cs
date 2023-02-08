using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public partial class HexTile
{
    public class MorphRule
    {
        public HexType resultType;
        public Dictionary<HexType, int> requiredNeighbours;

        public MorphRule(HexType resultType, params (HexType, int)[] requiredNeighbours)
        {
            this.resultType = resultType;
            this.requiredNeighbours =
                requiredNeighbours.ToDictionary(tup => tup.Item1, tup => tup.Item2);
        }

        public HexType? TryToMorph(CubeMap cubeMap, Vector3Int coord)
        {
            if (CheckNeighbours(cubeMap, coord))
                return resultType;

            return null;
        }

        public bool CheckNeighbours(CubeMap cubeMap, Vector3Int coord)
        {
            Dictionary<HexType, int> count = new Dictionary<HexType, int>(requiredNeighbours);

            var neighbours = cubeMap.NeighbourTilesInRadius(1, coord);

            foreach (var item in neighbours)
            {
                if (!count.ContainsKey(item.currentType))
                    return false;

                count[item.currentType]--;
            }

            return count.Values.Count(v => v > 0) == 0;
        }
    }
}
