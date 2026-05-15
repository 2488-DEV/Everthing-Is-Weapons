using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCard", menuName = "Scriptable Objects/UpgradeCard")]
public class UpgradeCard : ScriptableObject
{
    public string cardName;
    public string cardDescription;
    public float additivePercent;
    public int starCount;
    public Sprite cardIMG;
}
