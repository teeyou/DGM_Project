using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlideAnim : MonoBehaviour
{
    [SerializeField] private RectTransform _rt;
    [SerializeField] private float _duration;
    [SerializeField] private Vector2 _startPos;
    [SerializeField] private Vector2 _targetPos;

    private Coroutine _slideRoutine = null;

    public void SlideIn()
    {
        if (_slideRoutine == null)
        {
            _slideRoutine = StartCoroutine(CoSlideIn(_duration, _targetPos, true));
        }

        else
        {
            StopCoroutine(_slideRoutine);
            _slideRoutine = StartCoroutine(CoSlideIn(_duration, _targetPos, true));
        }
    }

    public void SlideOut()
    {
        if (_slideRoutine == null)
        {
            _slideRoutine = StartCoroutine(CoSlideIn(_duration, _startPos, false));
        }
        else
        {
            StopCoroutine(_slideRoutine);
            _slideRoutine = StartCoroutine(CoSlideIn(_duration, _startPos, true));
        }
    }

    private IEnumerator CoSlideIn(float duration, Vector2 targetPos, bool slideIn)
    {
        Vector2 startPos = _rt.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            _rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _rt.anchoredPosition = targetPos;

        _slideRoutine = null;
    }
}
