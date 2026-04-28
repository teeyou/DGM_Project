using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : Singleton<GameManager>
{
    private Transform _playerTr;
    private EventChannel _eventChannel;

    private AsyncOperationHandle<EventChannel> _handle;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _handle = Addressables.LoadAssetAsync<EventChannel>("EventChannel");
        _handle.Completed += OnEventChannelLoaded;
    }

    private void OnDisable()
    {
        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
        }
    }

    private void OnEventChannelLoaded(AsyncOperationHandle<EventChannel> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _eventChannel = handle.Result;

            if (_playerTr == null)
            {
                Debug.Log("GameManager : SetPlayer");
                _eventChannel.OnPlayerSpawned += SetPlayer;
            }
        }
    }

    void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");

        if (go != null)
        {
            SetPlayer(go);
        }
    }

    private void SetPlayer(GameObject player)
    {
        _playerTr = player.transform;
    }

    public void SetPlayerPosition(ESceneId current, ESceneId target)
    {
        string cur = current.ToString();
        string tar = target.ToString();
        string point = cur + "To" + tar;

        Transform pointTr = GameObject.Find(point).transform;

        if (pointTr != null && _playerTr != null)
        {
            _playerTr.position = pointTr.position;
            _playerTr.rotation = pointTr.rotation;

            MoveController controller = _playerTr.GetComponent<MoveController>();

            if (controller != null)
            {
                controller.ResetCC();
            }
        }
    }

    public bool IsExistsPlayer() => _playerTr != null;
}
