using UnityEngine;

[CreateAssetMenu(fileName = "WeaponType", menuName = "Scriptable Objects/WeaponType")]
public class WeaponType : ScriptableObject
{
    public string typeName;
    public float baseAttackSpeed;
    public float baseReach;
}
