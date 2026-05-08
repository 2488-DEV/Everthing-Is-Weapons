using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clickSfx; // เสียงคลิกทั่วไป
    public AudioClip backSfx;  // (เพิ่ม) เผื่อมีเสียงตอนกด "ย้อนกลับ" กวัก

    // ฟังก์ชันเล่นเสียงคลิกมาตรฐาน
    public void PlayClick()
    {
        if (source != null && clickSfx != null)
        {
            // เราเล่นผ่าน AudioSource ที่ต่อ Output ไปที่ Mixer ไว้แล้ว
            // เสียงจะเงียบตามหลอด SFX โดยอัตโนมัติกวัก!
            source.PlayOneShot(clickSfx);
        }
    }

    // ฟังก์ชันเล่นเสียงกดย้อนกลับ (เผื่อนายอยากให้เสียงไม่เหมือนกันกวัก)
    public void PlayBack()
    {
        if (source != null && backSfx != null)
        {
            source.PlayOneShot(backSfx);
        }
    }

    // ฟังก์ชันพิเศษ: เล่นเสียงอะไรก็ได้ที่ส่งเข้ามา (เผื่อใช้กับเสียงอื่นๆ ใน UI)
    public void PlayCustomSound(AudioClip clip)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip);
        }
    }
}