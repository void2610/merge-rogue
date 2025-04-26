using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            // すでに存在していればそのインスタンスを返す
            if (!_instance)
            {
                // インスタンスが存在しない場合は自動生成する
                var singletonObject = new GameObject(typeof(T).Name);
                _instance = singletonObject.AddComponent<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // 自分自身が初めてのインスタンスなら登録し、シーン遷移時にも破棄されないよう設定
        if (!_instance)
        {
            _instance = this as T;
        }
        // 既に存在している（別のインスタンスが登録済み）の場合は自分自身を破棄
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}