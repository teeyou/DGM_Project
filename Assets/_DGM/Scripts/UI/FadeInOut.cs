using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOut : Singleton<FadeInOut>
{
    [SerializeField] private CanvasGroup _cg;
    [SerializeField] private float _duration;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
    }

    private async UniTask FadeAsync(float start, float end, float duration)
    {
        float elapsed = 0f;
        _cg.alpha = start;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            _cg.alpha = Mathf.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        _cg.alpha = end;
    }

    public void FadeIn()
    {
        FadeAsync(0f, 1f, _duration).Forget();
    }

    public void FadeOut()
    {
        FadeAsync(1f, 0f, _duration).Forget();
    }

    public async UniTask FadeOutAsync()
    {
        await FadeAsync(1f, 0f, _duration);
    }

    public async UniTask FadeInAsync()
    {
        await FadeAsync(0f, 1f, _duration);
    }
}
