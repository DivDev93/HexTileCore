using UnityEngine;

[CreateAssetMenu(fileName = "ElementalStatModifiers", menuName = "HexTileCore/ElementalStatModifiers", order = 1)]
public class ElementalStatModifiersScriptableObject : ScriptableObject
{
    public float attackMultiplier = 1.5f;
    public float defenseMultiplier = 1.5f;
    public float speedMultiplier = 1.5f;
}