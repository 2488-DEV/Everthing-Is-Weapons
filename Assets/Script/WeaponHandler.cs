using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public WeaponInfo currentWeapon;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateWeaponVisual();
    }

    public void SetWeapon(WeaponInfo newWeapon)
    {
        currentWeapon = newWeapon;
        UpdateWeaponVisual();
    }

    void UpdateWeaponVisual()
    {
        if (currentWeapon == null) {
            Debug.LogError("ยังไม่ได้ใส่ไฟล์ SO ใน WeaponHandler นะเจมส์!");
            return;
        }
        
        if (sr == null) {
            Debug.LogError("ลืมแปะ SpriteRenderer ไว้ที่ WeaponParent หรือเปล่า?");
            return;
        }
    
        sr.sprite = currentWeapon.itemModel;
        Debug.Log("เปลี่ยนรูปอาวุธเป็น: " + currentWeapon.weaponName);
    }
}