using UnityEngine;

[CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Objects/EnemyInfo")]
public class EnemyInfo : ScriptableObject
{
    public float health;
    public float speed;
    public float attackDamage;
    public Sprite enemyModel;
    public float senseLocate;
    public float attackSpeed;
    public float attackReach;
}
