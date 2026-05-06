using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextSpacingAnim : MonoBehaviour
{
    public event Action OnAnimationFinished;

    [SerializeField] private float _targetSpacing;
    [SerializeField] private float _duration;
    [SerializeField] private bool _replay = false;
    private TMP_Text _tmp;
    
    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        _tmp.characterSpacing = 0;
    }
    void Start()
    {
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        StartCoroutine(CoPlayAnimation());
    }

    private IEnumerator CoPlayAnimation()
    {
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _tmp.characterSpacing = Mathf.Lerp(0f, _targetSpacing, t);
            yield return null;
        }

        _tmp.characterSpacing = _targetSpacing;
        OnAnimationFinished?.Invoke();
    }

    private void Update()
    {
        if (_replay)
        {
            _replay = false;
            PlayAnimation();
        }
    }
}
