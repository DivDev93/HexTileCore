using Reflex.Attributes;
using UnityEngine;
public enum OperatorType { Add, Multiply }

[RequireComponent(typeof(AudioSource))]
public class StatModifierPickup : Pickup {

    [Inject]
    IStatModifierFactory statModifierFactory;
    // TODO Move configuration to ScriptableObject
    [SerializeField] EStatType type = EStatType.Attack;
    [SerializeField] OperatorType operatorType = OperatorType.Add;
    [SerializeField] int value = 10;
    [SerializeField] float duration = 5f;

    protected override void ApplyPickupEffect(Entity entity) {
        StatModifier modifier = statModifierFactory.Create(operatorType, type, value, duration);
        entity.Stats.Mediator.AddModifier(modifier);
    }
}