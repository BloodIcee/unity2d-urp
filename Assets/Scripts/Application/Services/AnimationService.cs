using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

public class AnimationService
{
    private readonly GameConfig config;

    public AnimationService(GameConfig gameConfig)
    {
        config = gameConfig;
    }

    public async UniTask AnimateCardFlip(CardView card, bool reveal, CancellationToken cancellationToken)
    {
        if (card == null)
            return;

        RectTransform targetTransform = reveal ? card.BaseImage?.rectTransform : card.BackImage?.rectTransform;
        RectTransform hideTransform = reveal ? card.BackImage?.rectTransform : card.BaseImage?.rectTransform;

        if (targetTransform == null)
            return;

        float halfDuration = config.CardFlipDuration / 2f;

        Sequence flipSequence = DOTween.Sequence();

        if (hideTransform != null && hideTransform.gameObject.activeSelf)
        {
            flipSequence.Append(
                hideTransform.DORotate(new Vector3(0, 90, 0), halfDuration, RotateMode.Fast)
                    .SetEase(Ease.InQuad)
            );

            flipSequence.AppendCallback(() => {
                hideTransform.gameObject.SetActive(false);
                targetTransform.gameObject.SetActive(true);
                targetTransform.localRotation = Quaternion.Euler(0, -90, 0);
            });

            flipSequence.Append(
                targetTransform.DORotate(Vector3.zero, halfDuration, RotateMode.Fast)
                    .SetEase(Ease.OutQuad)
            );
        }
        else
        {
            targetTransform.gameObject.SetActive(true);
            targetTransform.localRotation = Quaternion.Euler(0, -90, 0);
            
            flipSequence.Append(
                targetTransform.DORotate(Vector3.zero, halfDuration, RotateMode.Fast)
                    .SetEase(Ease.OutQuad)
            );
        }

        flipSequence.SetLink(card.gameObject);
        await WaitForTween(flipSequence, cancellationToken);
    }

    public async UniTask AnimateMatch(CardView card, CancellationToken cancellationToken)
    {
        if (card == null || card.BaseImage == null)
            return;

        RectTransform transform = card.BaseImage.rectTransform;
        Vector3 originalScale = transform.localScale;

        Sequence matchSequence = DOTween.Sequence();

        matchSequence.Append(
            transform.DOScale(originalScale * 1.15f, 0.2f)
                .SetEase(Ease.OutBack)
        );

        matchSequence.Append(
            transform.DOScale(originalScale, 0.3f)
                .SetEase(Ease.InOutElastic)
        );

        matchSequence.Join(
            transform.DORotate(new Vector3(0, 0, 10), 0.15f)
                .SetLoops(2, LoopType.Yoyo)
        );

        matchSequence.SetLink(card.gameObject);
        await WaitForTween(matchSequence, cancellationToken);
    }

    public async UniTask AnimateMismatch(CardView card, CancellationToken cancellationToken)
    {
        if (card == null)
            return;

        RectTransform transform = card.transform as RectTransform;
        if (transform == null)
            return;

        Vector3 originalPosition = transform.localPosition;

        Sequence mismatchSequence = DOTween.Sequence();

        mismatchSequence.Append(
            transform.DOShakePosition(0.4f, strength: 15f, vibrato: 20)
                .SetEase(Ease.Linear)
        );

        mismatchSequence.AppendCallback(() => {
            transform.localPosition = originalPosition;
        });

        mismatchSequence.SetLink(card.gameObject);
        await WaitForTween(mismatchSequence, cancellationToken);
    }

    public async UniTask AnimateCardSpawn(CardView card, float delay, CancellationToken cancellationToken)
    {
        if (card == null)
            return;

        RectTransform transform = card.transform as RectTransform;
        if (transform == null)
            return;

        transform.localScale = Vector3.zero;
        
        CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;

        await UniTask.Delay((int)(delay * 1000), cancellationToken: cancellationToken);

        Sequence spawnSequence = DOTween.Sequence();

        spawnSequence.Append(
            transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
        );

        spawnSequence.Join(
            canvasGroup.DOFade(1f, 0.3f)
                .SetEase(Ease.OutQuad)
        );

        spawnSequence.SetLink(card.gameObject);
        await WaitForTween(spawnSequence, cancellationToken);
    }

    public async UniTask AnimateGridHide(List<CardView> cards, CancellationToken cancellationToken)
    {
        List<UniTask> tasks = new List<UniTask>();
        
        foreach (var card in cards)
        {
            if (card == null) continue;
            
            RectTransform transform = card.transform as RectTransform;
            if (transform == null) continue;

            Tween scaleTween = transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            tasks.Add(WaitForTween(scaleTween, cancellationToken));
        }

        await UniTask.WhenAll(tasks);
    }

    private async UniTask WaitForTween(Tween tween, CancellationToken cancellationToken)
    {
        var completionSource = new UniTaskCompletionSource();
        
        tween.OnComplete(() => completionSource.TrySetResult());
        tween.OnKill(() => completionSource.TrySetCanceled());

        using (cancellationToken.Register(() => {
            tween.Kill();
            completionSource.TrySetCanceled();
        }))
        {
            await completionSource.Task;
        }
    }
}
