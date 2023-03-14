using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Collections;

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
        static public Dictionary<TKey, TValue> ToDict<TKey, TValue>(this IEnumerable<(TKey, TValue )> source)
        {
            return source.ToDictionary(kv => kv.Item1,
                                       kv => kv.Item2);
        }
        static public Dictionary<TKey, TValue> ToDict<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> source)
        {
            return source.ToDictionary(kv => kv.Item1,
                                       kv => kv.Item2);
        }
        static public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToDictionary(kv => kv.Key,
                                       kv => kv.Value);
        }
        static public IEnumerable<( int, T )> Enumerate<T>(this IEnumerable<T> source)
        {
            int i = 0;
            foreach (var item in source)
            {
                yield return (i, item);
                i++;
            }
        }
        static public IEnumerable<T> Map<T>(this IEnumerable<T> source, Action<T> action)
        {
            return
                source.ToList()
                .Select(item =>
                {
                    action.Invoke(item);
                    return item;
                });
        }
        static public IEnumerable<(T, O)> Zip<T, O>(this IEnumerable<T> source, IEnumerable<O> other)
        {
            return source.Zip(other, (a , b) => (a, b));
        }
        static public IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(_ => UnityEngine.Random.value);
        }
        static public T GetRandomOrThrow<T>(this IEnumerable<T> source)
        {
            int count = source.Count();

            if (count == 0)
                throw new System.Exception("No items in collection!");

            int id = UnityEngine.Random.Range(0, count);

            return source.ElementAt(id);
        }
        static public T GetRandomOrDefault<T>(this IEnumerable<T> source)
        {
            int count = source.Count();

            if (count == 0)
                return default;

            int id = UnityEngine.Random.Range(0, count);

            return source.ElementAt(id);
        }
        static public IEnumerable<T> NotEqual<T>(this IEnumerable<T> source, T other)
        {
            return source.Where(item => !item.Equals(other));
        }
        static public IEnumerable<T> Log<T>(this IEnumerable<T> source, string prefixMessage="")
        {
            string str =
                (!source.Any())
                ? "Empty"
                : source
                .Select(item => item.ToString())
                .Aggregate((a, b) => a + ", " + b);

            Debug.Log(prefixMessage + ": " + str);

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

    static public class IObservableExt
    {
        static public IObservable<bool> IsTrue(this IObservable<bool> source)
        {
            return source.Where(b => b == true);
        }
        static public IObservable<bool> IsFalse(this IObservable<bool> source)
        {
            return source.Where(b => b == false);
        }
    }
}

static public class NetworkConnectionExt
{
    static public T GetSingleOwnedOfType<T>(this Mirror.NetworkConnection connection)
        where T : Component
    {
        return
            connection.owned.ToList()
            .Single(netid => netid.GetComponent<T>())
            .GetComponent<T>();
    }
}

public static class MonoBehaviourExtension
{
    public static Coroutine StartInvokeAfter(this MonoBehaviour mono, Action action, float seconds)
    {
        return mono.StartCoroutine(CoroutineExtension.InvokeAfter( action,  seconds));
    }

    public static Coroutine StartSearchForObjectOfType<T>(this MonoBehaviour mono, Action<T> onFound)
        where T : Component
    {
        return mono.StartCoroutine(CoroutineExtension.SearchForObjectOfType<T>(onFound));
    }
}

public static class CoroutineExtension
{
    public static System.Collections.IEnumerator InvokeAfter(Action action, float seconds)
    {
        var wait = new WaitForEndOfFrame();

        while ((seconds -= Time.deltaTime) > 0f) yield return wait;

        action.Invoke();
    }
    public static IEnumerator SearchForObjectOfType<T>(Action<T> onFound)
        where T : Component
    {
        T obj = null;
        do
        {
            yield return new WaitForEndOfFrame();

            obj = GameObject.FindObjectOfType<T>();

            if (obj != null)
            {
                onFound.Invoke(obj);
                yield break;
            }
        }
        while (obj == null);
    }

}

public static class CanvasGroupExtension
{
    static public void SetVisibleAndInteractable(this CanvasGroup canvasGroup, bool visible)
    {
        canvasGroup.alpha = (visible) ? 1f : 0f;
        SetInteractable(canvasGroup, visible);
    }

    private static void SetInteractable(this CanvasGroup canvasGroup, bool visible)
    {
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
