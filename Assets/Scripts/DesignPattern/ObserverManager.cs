using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ObserverManager<T> where T : Enum
{
    private static Dictionary<T, Action<object>> _boardObserver = new Dictionary<T, Action<object>>();

    public static void AddRegisterEvent(T eventID, Action<object> callback)
    {
        if (callback == null)
        {
            return;
        }
        if (eventID == null)
        {
            return;
        }
        if (!_boardObserver.TryAdd(eventID, callback))
        {
            _boardObserver[eventID] += callback;
            return;
        }
    }

    public static void PostEvent(T eventID, object param = null)
    {
        if (!_boardObserver.ContainsKey(eventID))
        {
            Debug.LogWarning("Chưa có class nào đăng ký sự kiện");
            return;
        }
        if (_boardObserver[eventID] == null)
        {
            _boardObserver.Remove(eventID);
            Debug.LogWarning("Event null");
            return;
        }
        _boardObserver[eventID]?.Invoke(param);
    }

    public static void RemoveAddListener(T eventID, Action<object> callback)
    {
        if (!_boardObserver.ContainsKey(eventID))
        {
            return;
        }
        _boardObserver[eventID] -= callback;
        if (_boardObserver[eventID] == null)
        {
            _boardObserver.Remove(eventID);
            return;
        }
    }

    public static void RemoveAllEvent()
    {
        _boardObserver.Clear();
    }
}
