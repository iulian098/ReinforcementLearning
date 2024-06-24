using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventsManager
{
    public delegate void EventDelegate<T>(T e) where T : GameEvent;

    static Dictionary<Type, Delegate> events = new Dictionary<Type, Delegate>();

    public static void AddListener<T>(EventDelegate<T> _delegate) where T : GameEvent {
        if (events.ContainsKey(typeof(T))) {
            Delegate tmp = events[typeof(T)];
            events[typeof(T)] = Delegate.Combine(tmp, _delegate);
        }else
            events[typeof(T)] = _delegate;
    }

    public static void RemoveListener<T>(EventDelegate<T> _delegate) where T : GameEvent {
        if (!events.ContainsKey(typeof(T)))
            return;

        Delegate currentDel = Delegate.Remove(events[typeof(T)], _delegate);

        if (currentDel == null)
            events.Remove(typeof(T));
        else
            events[typeof(T)] = currentDel;
    }

    public static void Raise(GameEvent e) {
        if (e == null) {
            Debug.Log("Invalid event argument: " + e.GetType().ToString());
            return;
        }

        if (events.ContainsKey(e.GetType()))
            events[e.GetType()].DynamicInvoke(e);
    }
}
