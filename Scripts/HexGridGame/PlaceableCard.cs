using Reflex.Attributes;
using UnityEngine;

public class PlaceableCard : BoardPlaceable
{
    [Inject]
    IStaticEvents staticEvents;

    bool placedForFirstTime = false;

    public override void OnTargetPlace()
    {
        base.OnTargetPlace();
        if (!placedForFirstTime)
        {
            placedForFirstTime = true;
            staticEvents.OnTurnEnd(0);
        }
    }

}
