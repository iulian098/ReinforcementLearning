using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool IsNullOrEmpty<T>(this IList<T> list) {
        return list == null || list.Count == 0;
    }

    public static bool IsNullOrEmpty<T, U>(this IDictionary<T, U> dictionary) {
        return dictionary == null || dictionary.Count == 0;
    }

    public static U Get<T, U>(this IDictionary<T, U> dic, T key) where U : new() {
        if (!dic.ContainsKey(key))
            dic.Add(key, new U());

        return dic[key];
    }

    public static U Get<T, U>(this IDictionary<T, U> dic, T key, U defaultVal) where U : new() {
        if (!dic.ContainsKey(key))
            dic.Add(key, defaultVal);

        return dic[key];
    }

    public static void Set<T, U>(this Dictionary<T, U> dic, T key, U val) {
        if(!dic.ContainsKey(key))
            dic.Add(key, val);
        else
            dic[key] = val;
    }
}
