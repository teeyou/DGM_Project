using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAlphaAnim : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private float _targetAlpha;

    private TMP_Text _tmp;

    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
        _tmp.alpha = 1f;
    }

    private void Start()
    {
        StartCoroutine(CoPlayAnimation());
    }

    private IEnumerator CoPlayAnimation()
    {
        while (true)
        {
            yield return StartCoroutine(CoAnimateAlpha(1f, _targetAlpha));

            yield return StartCoroutine(CoAnimateAlpha(_targetAlpha, 1f));

        }
    }

    private IEnumerator CoAnimateAlpha(float start, float end)
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _tmp.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        _tmp.alpha = end;
    }
}
