using UnityEngine;

public static class InputHelper
{
    public static bool GetPointerDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.GetMouseButtonDown(0);
#else
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#endif
    }

    public static bool GetPointerHeld()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.GetMouseButton(0);
#else
        return Input.touchCount > 0 &&
               (Input.GetTouch(0).phase == TouchPhase.Moved ||
                Input.GetTouch(0).phase == TouchPhase.Stationary);
#endif
    }

    public static bool GetPointerUp()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.GetMouseButtonUp(0);
#else
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
#endif
    }

    public static Vector2 GetPointerScreenPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.mousePosition;
#else
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;
        return Vector2.zero;
#endif
    }

    public static Vector3 GetPointerWorldPosition()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(GetPointerScreenPosition());
        worldPos.z = 0f;
        return worldPos;
    }
}
