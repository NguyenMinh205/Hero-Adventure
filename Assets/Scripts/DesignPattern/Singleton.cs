using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool dontDestroyOnLoad;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>(true);
                if (instance == null)
                {
                    GameObject singleton = new GameObject();
                    instance = singleton.AddComponent<T>();
                    singleton.name = typeof(T).Name;
                    if (dontDestroyOnLoad)
                    {
                        DontDestroyOnLoad(singleton);
                    }
                }
            }
            return instance;
        }
    }
    protected virtual void KeepActive(bool enable)
    {
        dontDestroyOnLoad = enable;
    }
    protected virtual void Awake()
    {
        if (instance != null && instance.GetInstanceID() != GetInstanceID())
        {
            Destroy(this);
            return;
        }
        instance = (T)(MonoBehaviour)this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this);
        }
    }
}
