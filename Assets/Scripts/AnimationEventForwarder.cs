using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    public BossEnemy target; // drag your BossEnemy from parent here (or leave blank)

    public void AE_MeleeHit()
    {
        if (!target) target = GetComponentInParent<BossEnemy>();
        if (target) target.AE_MeleeHit();
    }

    public void AE_CastRelease()
    {
        if (!target) target = GetComponentInParent<BossEnemy>();
        if (target) target.AE_CastRelease();
    }
}
