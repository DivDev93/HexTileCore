using UnityEngine;
using PrimeTween;
using UnityEngine.Assertions;
using TMPro;

public static class PrimeTweenExtensions
{
    public static Sequence Jump(Transform target, Vector3 endValue, float duration, float height, int numJumps = 1)
    {
        Assert.IsTrue(height > 0f);
        Assert.IsTrue(numJumps >= 1, nameof(numJumps) + "should be >= 1.");
        var jumpsSequence = Sequence.Create();
        var iniPosY = target.position.y;
        var deltaJump = (endValue.y - iniPosY) / numJumps;
        var jumpDuration = duration / (numJumps * 2);
        for (int i = 0; i < numJumps; i++)
        {
            var from = iniPosY + i * deltaJump;
            var to = iniPosY + (i + 1) * deltaJump;
            jumpsSequence.Chain(Tween.PositionY(target, Mathf.Max(from, to) + height, jumpDuration, Ease.OutQuad))
                .Chain(Tween.PositionY(target, to, jumpDuration, Ease.InQuad));
        }
        var result = Sequence.Create()
            .Group(jumpsSequence);
        if (!Mathf.Approximately(target.position.x, endValue.x))
        {
            result.Group(Tween.PositionX(target, endValue.x, duration, Ease.Linear));
        }
        if (!Mathf.Approximately(target.position.z, endValue.z))
        {
            result.Group(Tween.PositionZ(target, endValue.z, duration, Ease.Linear));
        }
        return result;
    }

    public static Sequence Jump(Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration)
    {
        Assert.IsTrue(numJumps >= 1);
        var jumpsSequence = Sequence.Create();
        var iniPosY = target.position.y;
        var deltaJump = (endValue.y - iniPosY) / numJumps;
        var jumpDuration = duration / (numJumps * 2);
        for (int i = 0; i < numJumps; i++)
        {
            var from = iniPosY + i * deltaJump;
            var to = (i + 1) * deltaJump;
            jumpsSequence.Chain(Tween.PositionY(target, Mathf.Max(from, to) + jumpPower, jumpDuration, Ease.OutQuad))
                .Chain(Tween.PositionY(target, to, jumpDuration, Ease.InQuad));
        }
        Sequence seq = Tween.PositionX(target, endValue.x, duration, Ease.Linear)
            .Group(Tween.PositionZ(target, endValue.z, duration, Ease.Linear))
            .Group(Tween.PositionY(target, endValue.y, duration, Ease.Linear))
            .Group(jumpsSequence);

            seq.OnComplete(() =>
            {
                target.position = endValue;
            });

        return seq;
    }

    public static Sequence LocalJump(Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration)
    {
        Assert.IsTrue(numJumps >= 1);
        var jumpsSequence = Sequence.Create();
        var iniPosY = target.localPosition.y;
        var deltaJump = (endValue.y - iniPosY) / numJumps;
        var jumpDuration = duration / (numJumps * 2);
        for (int i = 0; i < numJumps; i++)
        {
            var from = iniPosY + i * deltaJump;
            var to = (i + 1) * deltaJump;
            jumpsSequence.Chain(Tween.LocalPositionY(target, Mathf.Max(from, to) + jumpPower, jumpDuration, Ease.OutQuad))
                .Chain(Tween.LocalPositionY(target, to, jumpDuration, Ease.InQuad));
        }
        return Tween.LocalPositionX(target, endValue.x, duration, Ease.Linear)
            .Group(Tween.LocalPositionZ(target, endValue.z, duration, Ease.Linear))
            .Group(Tween.LocalPositionY(target, endValue.y, duration, Ease.Linear))
            .Group(jumpsSequence);
    }

    public static Sequence Jump(this Transform target, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
    {
        if (numJumps < 1)
        {
            numJumps = 1;
        }

        float startPosY = target.position.y;
        float endPosY = jumpPower;
        Sequence s = PrimeTween.Sequence.Create();
        Tween yTween = PrimeTween.Tween.PositionY(target, jumpPower, duration / (numJumps * 2), Ease.OutQuad)
            .SetRelative()
            .SetLoops(numJumps * 2, LoopType.Yoyo);

        s.Append(PrimeTween.Tween.PositionX(target, endValue.x, duration, Ease.Linear))
         .Join(PrimeTween.Tween.PositionZ(target, endValue.z, duration, Ease.Linear))
         .Join(yTween)
         .SetEase(Ease.Linear);

        //yTween.OnUpdate(target, (t, tween) =>
        //{
        //    Vector3 position = t.position;
        //    position.y += Mathf.Lerp(0f, endPosY, tween.ElapsedPercentage());
        //    t.position = position;
        //});


        return s;
    }

    public static Tween PulseY(this Transform target, Vector3 startPos, float height, int numPulses = 1, float duration = 0.5f, bool isLocal = false)
    {
        if (numPulses < 1)
        {
            numPulses = 1;
        }
        float startPosY = startPos.y;
        float endPosY = startPos.y + height;
        Tween yTween = default;
        if(isLocal)
        {
            yTween = PrimeTween.Tween.LocalPositionY(target, endPosY, duration / (numPulses * 2), Ease.Linear);           
        }
        else
        {
            yTween = PrimeTween.Tween.PositionY(target, endPosY, duration / (numPulses * 2), Ease.Linear);
              
        }
        yTween.SetRelative()
            .SetLoops(numPulses * 2, LoopType.Yoyo);

       yTween.SetEase(Ease.Linear);
       yTween.OnComplete(() =>
        {
            if (isLocal)
                target.localPosition = startPos;
            else
                target.position = startPos;
        });

        return yTween;
    }

    public static Tween Counter(this TMP_Text text, int startValue, int endValue, float duration, Ease ease = Ease.Default)
    {
        return Tween.Custom(text, startValue, endValue, duration, (target, val) => target.SetText("{0:0}", val), ease);
    }
}
