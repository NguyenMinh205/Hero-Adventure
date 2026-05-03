using UnityEngine;

public class Player : BaseCharacter
{
    public override void BroadcastUIUpdate()
    {
        ObserverManager<EventID>.PostEvent(EventID.OnUpdatePlayerStats, this);
    }
}