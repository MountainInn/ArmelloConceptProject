using System;
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
        static public object GetRandom(this System.Array array)
        {
            int id = UnityEngine.Random.Range(0, array.Length);

            return array.GetValue(id);
        }
    }
}
