using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class DollyCameraMove : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _vcam;
    [SerializeField] private float speed;

    private CinemachineTrackedDolly _dolly;

    void Start()
    {
        _dolly = _vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }

    void Update()
    {
        if (_dolly != null)
        {
            _dolly.m_PathPosition += speed * Time.deltaTime;
        }
    }
}
