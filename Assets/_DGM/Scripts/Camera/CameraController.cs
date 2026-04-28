using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;
    
    private Transform _playerTr;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private void Start()
    {
        if (_playerTr == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");

            if (go != null)
            {
                SetPlayer(go);
            }
        }

        if (_playerTr == null)
        {
            FindPlayer(_token).Forget();
        }
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        _token = _cts.Token;
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void SetPlayer(GameObject player)
    {
        _playerTr = player.transform;
        _vcam.m_Follow = _playerTr;
    }

    private async UniTaskVoid FindPlayer(CancellationToken token)
    {
        try
        {
            await UniTask.WaitUntil(() => GameManager.Instance.IsExistsPlayer(), PlayerLoopTiming.Update, _token);
            SetPlayer(GameManager.Instance.GetPlayer());

            //await UniTask.WaitUntil(() =>
            //{
            //    GameObject go = GameObject.FindGameObjectWithTag("Player");

            //    if (go != null)
            //    {
            //        SetPlayer(go);
            //    }
            //    return go != null;
            //},
            //PlayerLoopTiming.Update, token);

            //_playerTr = GameObject.FindGameObjectWithTag("Player").transform;

        }

        catch (OperationCanceledException)
        {

        }
        catch (Exception e)
        {
            Debug.Log($"Error : {e.Message}");
        }
    }

    
}
