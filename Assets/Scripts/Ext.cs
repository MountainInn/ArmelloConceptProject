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

    static public class IntExt
    {
        static public void ForLoop(this int i, Action<int> action)
        {
            Enumerable.Range(0, i).ToList().ForEach(action);
        }
        static public IEnumerable<int> ToRange(this int i)
        {
            return Enumerable.Range(0, i);
        }
    }

    static public class IEnumerableExt
    {
        static public IEnumerable<T> LookAt<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action.Invoke(item);
                yield return item;
            }
        }
        static public IEnumerable<(T, O)> Zip<T, O>(this IEnumerable<T> source, IEnumerable<O> other)
        {
            return source.Zip(other, (a , b) => (a, b));
        }
        static public IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(_ => UnityEngine.Random.value);
        }
        static public T GetRandom<T>(this IEnumerable<T> source)
        {
            int id = UnityEngine.Random.Range(0, source.Count());

            return source.ElementAt(id);
        }
        static public IEnumerable<T> NotEqual<T>(this IEnumerable<T> source, T other)
        {
            return source.Where(item => !item.Equals(other));
        }
        static public IEnumerable<T> Write<T>(this IEnumerable<T> source)
        {
            var str = source
                .Select(item => item.ToString())
                .Aggregate((a, b) => a + ", " + b);

            Debug.Log(str);

            return source;
        }
    }
    static public class EnumerableBoolExt
    {
        static public IEnumerable<bool> IsFalse(this IEnumerable<bool> source)
        {
            return source.Where(b => b == false);
        }
        static public IEnumerable<bool> IsTrue(this IEnumerable<bool> source)
        {
            return source.Where(b => b == true);
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

    public static class BoolExt
    {
        static public bool All(bool a, bool b, bool c)
        {
            return a && b && c;
        }
        static public bool All(bool a, bool b)
        {
            return a && b;
        }
    }


}
