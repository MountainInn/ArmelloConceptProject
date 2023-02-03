using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MountainInn
{
    static public class Ext
    {
        static private Canvas _canvas;
        static private Canvas canvas => _canvas ?? (_canvas = GameObject.FindObjectOfType<Canvas>());
           
        static public T GetRandom<T>(this List<T> list)
        {
            int id = UnityEngine.Random.Range(0, list.Count);

            return list[id];
        }

        static public Vector3 MousePositionScaledToCanvas()
        {
            return Input.mousePosition * canvas.transform.lossyScale.x;
        }
    }

    static public class GridExt
    {

        static public void HexXBorders(int radius, int y, out int left, out int right)
        {
            int absY = Math.Abs(y);

            left = -radius + (absY)  /2;
            right = radius - (absY+1)/2;
        }
    }

    static public class Vector3IntExt
    {
        static public Vector2Int xy(this Vector3Int v3) => new Vector2Int(v3.x, v3.y);
    }
   
    static public class Vector2IntExt
    {
        static public Vector3Int xy_(this Vector2Int v2, int z = 0) => new Vector3Int(v2.x, v2.y, z);
    }

    static public class ArrayExt
    {
        static public T ArrayGetRandom<T>(this System.Array array)
        {
            int id = UnityEngine.Random.Range(0, array.Length);

            return (T) array.GetValue(id);
        }
    }

    static public class IEnumerableExt
    {
        static public IEnumerable<T> NotNull<T>(this IEnumerable<T> enumerable)
            where T : class
        {
            return enumerable.Where(item => item != null);
        }
        static public T GetRandom<T>(this IEnumerable<T> enumerable)
        {
            int id = UnityEngine.Random.Range(0, enumerable.Count());

            return enumerable.ElementAt(id);
        }
    }

static public class MathExt
{
    static public int Fact(int n)
    {
        return
            Enumerable
            .Range(1, n)
            .Aggregate((a , b) => a * b);
    }
}

}
