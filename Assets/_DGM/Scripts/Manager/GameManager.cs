using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : Singleton<GameManager>
{
    private List<GameObject> _playerDigimonList = new List<GameObject>();
    private Transform _playerTr;
    private MoveController _playerMoveController;

    private EventChannel _eventChannel;

    private AsyncOperationHandle<EventChannel> _handle;

    public bool IsBlockInteractionKey { get; set; } = false;
    public bool IsPlayerInteracting { get; set; } = false;
    public bool HasDigimon { get; set; } = false;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _handle = Addressables.LoadAssetAsync<EventChannel>("EventChannel");
        _handle.Completed += OnEventChannelLoaded;
        DigimonSpawner.Instance.OnDigimonSpawned += OnDigimonSpawned;
    }

    private void OnDisable()
    {
        if (_handle.IsValid())
        {
            Addressables.Release(_handle);
        }
    }

    private void OnDigimonSpawned(GameObject digimon, bool isEnemy)
    {
        if (!isEnemy)
        {
            AddDigimon(digimon);
        }
    }

    public void AddDigimon(GameObject digimon)
    {
        _playerDigimonList.Add(digimon);

        Debug.Log($"AddDigimon {digimon.name}");
        Debug.Log($"_playerDigimonList {_playerDigimonList.Count}");
    }

    public IReadOnlyList<GameObject> GetDigimonList()
    {
        return _playerDigimonList.AsReadOnly();
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

        Vector3 pos = new Vector3(-10f, 0f, -10f);
        Quaternion rot = Quaternion.identity;
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

            if (_playerMoveController == null)
            {
                _playerMoveController = _playerTr.GetComponent<MoveController>();
            }

            _playerMoveController.ResetCC();
        }
    }

    public void TogglePlayerMoveController(bool enabled)
    {
        if (_playerMoveController == null)
        {
            _playerMoveController = _playerTr.GetComponent<MoveController>();
        }

        _playerMoveController.enabled = enabled;
    }

    public bool IsExistsPlayer() => _playerTr != null;

    public GameObject GetPlayer() => _playerTr.gameObject;
}
