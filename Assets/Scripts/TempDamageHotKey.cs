using UnityEngine;

public class TempDamageHotKey : MonoBehaviour
{
    public BossEnemyPlayerHealth playerHP;
    void Update() { if (Input.GetKeyDown(KeyCode.L)) playerHP.TakeDamage(20); }
}
