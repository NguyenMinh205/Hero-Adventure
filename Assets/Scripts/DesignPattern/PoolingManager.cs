using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class PoolingManager
{
    private static Dictionary<int, Pool> _pools; // Dictionary chứa key và value của lớp pool tức là một gameObject có ID khác nhau thì quản lí một pool khác

    private static void Init(GameObject prefab)
    {
        // Hàm khởi tạo ban đầu
        if (_pools == null)
        {
            _pools = new Dictionary<int, Pool>();
        }

        if (!_pools.ContainsKey(prefab.GetInstanceID()))
        {
            _pools[prefab.GetInstanceID()] = new Pool(prefab);
        }
    }

    public static GameObject Spawn(GameObject prefab)
    {
        Init(prefab); // Khởi tạo nếu như nó chưa tồn tại thì khởi tạo, nếu như đã có rồi thì bỏ qua cái này 
        return _pools[prefab.GetInstanceID()].Spawn(Vector3.zero, quaternion.identity);
    }
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion quaternion, Transform parent = null)
    {
        Init(prefab);
        return _pools[prefab.GetInstanceID()].Spawn(position, quaternion, parent);
    }
    public static T Spawn<T>(T prefab) where T : Component
    {
        Init(prefab.gameObject);
        return _pools[prefab.gameObject.GetInstanceID()].Spawn<T>(Vector3.zero, quaternion.identity);
    }
    public static T Spawn<T>(T prefab, Vector3 position, Quaternion quaternion, Transform parent = null) where T : Component
    {
        Init(prefab.gameObject);
        return _pools[prefab.gameObject.GetInstanceID()].Spawn<T>(position, quaternion, parent);
    }

    public static void Despawn(GameObject prefab)
    {
        if (!prefab.activeSelf) // Kiemr tra xem prefab được despawn có đang được active không nếu như không thì return
        {
            return;
        }
        Pool p = null; // Khởi tạo một Pool dùng kiểm tra xem Pool đó có tồn tại của Prefab nó có tồn tại không
        foreach (var pool in _pools.Values) // Dùng foreach duyệt qua tất cả value của dic Pool 
        {
            if (pool.IDObjects.Contains(prefab.GetInstanceID())) // Kiểm tra pool trong Dic và ID của prefab nó có tồn tại trong danh sách ID của Pool không, nếu có thì p = pool đó rồi thoát vòng lặp
            {
                p = pool;
                break;
            }
        }
        if (p != null) // p tồn tại thì despawn game object
        {
            p.Despawn(prefab);
        }
        else
        {
            Object.Destroy(prefab); // nếu p không tồn tại thì Destroy GameObject
        }
    }
}

public class Pool // Lớp cơ sở của Pooling
{
    private readonly Queue<GameObject> listGameObject; // Danh lưu trữ các prefab và chỉ được phép đọc
    public readonly HashSet<int> IDObjects; // Danh sách lưu trữ ID của các game object ==> dùng để kiểm tra xem ID của GameObject này có tồn tại hay không
    private GameObject prefab;  // GameObject prafabs

    public Pool(GameObject gameObject)
    {
        // Constructor khai báo lớp cơ sở
        prefab = gameObject;
        listGameObject = new Queue<GameObject>();
        IDObjects = new HashSet<int>();
    }

    public GameObject Spawn(Vector3 position, Quaternion quaternion, Transform parent = null)
    {
        // Hàm Spawn, Cho phép truyền vào một vị trí (postion), góc độ ban đầu (quaternion)  và thằng cha của nó ( mặc cha của là null)
        GameObject newObject; // Khai báo GameObject mới

        // Sử dụng while(true) để kiểm tra xem List GameObject có còn phần tử nào không, Nếu còn thì lấy từ List GameObject ra, Còn nếu không còn thì Sinh ra một GameObject mới
        while (true)
        {
            if (listGameObject.Count <= 0)
            {
                newObject = Object.Instantiate(prefab, position, quaternion, parent); //Sinh ra Object mới không có trong List Object
                newObject.name = prefab.name; // Set lại tên của nó
                IDObjects.Add(newObject.GetInstanceID()); // Thêm ID của nó vào danh sách ID
                return newObject; // Trả về gameObject được sinh ra
            }
            newObject = listGameObject.Dequeue(); // Lấy GameObject từ List GameObject 
            if (newObject == null) // Skip nếu object đã bị destroy (Unity treats destroyed as == null)
            {
                continue; // Pop tiếp object khác hoặc instantiate mới nếu hết
            }
            newObject.transform.SetPositionAndRotation(position, quaternion); // Set lại vị trí và góc độ đồng thời set lại gameobject chứa newObject
            newObject.transform.parent = parent;
            newObject.SetActive(true); //Tái kích hoạt nó
            return newObject;
        }
    }

    // Sử generic để tái sử dụng hàm spawn
    public T Spawn<T>(Vector3 position, Quaternion quaternion, Transform parent = null) where T : Component  // Điều kiện thằng T là generic và T phải là một Component, Tức là T là một thành phần của GameObject
    {
        return Spawn(position, quaternion, parent).GetComponent<T>(); // trả về kiểu dữ liệu T 
    }

    public void Despawn(GameObject gameObject)
    {
        if (gameObject == null) return;
        gameObject.SetActive(false); // Tắt game obejct
        listGameObject.Enqueue(gameObject); // Đẩy game object vào danh sách object
    }
}
