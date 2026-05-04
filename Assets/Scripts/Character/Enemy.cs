using System.Collections;
using UnityEngine;

public class Enemy : BaseCharacter
{
    public override void BroadcastUIUpdate()
    {
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateEnemyHP, this);
    }

    protected override void DestroyOrDespawn()
    {
        StartCoroutine(DespawnAfterDelay(1f));
    }

    private IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolingManager.Despawn(gameObject);
    }
}