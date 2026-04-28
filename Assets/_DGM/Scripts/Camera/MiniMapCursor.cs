using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MiniMapCursor : MonoBehaviour
{
    private EventChannel _eventChannel;

    private Transform _playerTr;

    private void OnEnable()
    {
        Addressables.LoadAssetAsync<EventChannel>("EventChannel").Completed += OnEventChannelLoaded;
    }

    private void OnEventChannelLoaded(AsyncOperationHandle<EventChannel> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _eventChannel = handle.Result;
            _eventChannel.OnPlayerSpawned += SetPlayer;
        }
    }

    private void SetPlayer(GameObject player)
    {
        _playerTr = player.transform;
    }

    private void Update()
    {
        if (_playerTr == null)
            return;

        transform.rotation = _playerTr.rotation;
    }
}
