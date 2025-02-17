using UnityEngine;

[CreateAssetMenu(fileName = "BaseStats", menuName = "Stats/BaseStats")]
public class BaseStats : ScriptableObject {
    public int attack = 2;
    public int defense = 2;
    public int speed = 2;
}