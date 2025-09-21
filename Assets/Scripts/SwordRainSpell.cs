using UnityEngine;

public class SwordRainSpell : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Transform target;

    [Header("Pattern")]
    public int swordCount = 8;
    public float radius = 2.5f;
    public float spawnHeight = 10f;

    [Header("Timing")]
    public float cooldown = 5f;

    static readonly int HashCast = Animator.StringToHash("Cast");
    bool isCasting;
    float nextReadyTime;

    void Reset() { animator = GetComponent<Animator>(); }

    public bool TryCast()
    {
        if (isCasting || Time.time < nextReadyTime) return false;
        isCasting = true;
        animator.SetTrigger(HashCast);
        return true;
    }

    // Animation Event on Cast Spell clip
    public void OnCastEmit()
    {
        if (!isCasting || !target) return;
        if (SpawnManager.Instance == null)
        {
            Debug.LogError("[Spell] SpawnManager.Instance is null — did you add it to the scene?");
            return;
        }
        Debug.Log("[Spell] OnCastEmit fired (manager)");
        SpawnManager.Instance.SpawnSwordRain(target.position, swordCount, radius, spawnHeight);
    }

    // StateMachineBehaviour OnStateExit OR a 2nd end-of-clip event calls this
    public void OnCastEnd()
    {
        Debug.Log("[Spell] OnCastEnd");
        isCasting = false;
        nextReadyTime = Time.time + cooldown;
    }
}
