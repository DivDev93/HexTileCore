using System;

public interface IStatModifierFactory {
    StatModifier Create(OperatorType operatorType, EStatType statType, int value, float duration);
    StatModifier Create(OperatorType operatorType, EStatType statType, float value, float duration);

}

public class StatModifierFactory : IStatModifierFactory {
    public StatModifier Create(OperatorType operatorType, EStatType statType, int value, float duration) {
        IOperationStrategy strategy = operatorType switch {
            OperatorType.Add => new AddOperation(value),
            OperatorType.Multiply => new MultiplyOperation(value),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return new StatModifier(statType, strategy, duration);
    }

    public StatModifier Create(OperatorType operatorType, EStatType statType, float value, float duration)
    {
        IOperationStrategy strategy = operatorType switch
        {
            OperatorType.Add => new AddOperation(((int)value)),
            OperatorType.Multiply => new MultiplyFloatOperation(value),
            _ => throw new ArgumentOutOfRangeException()
        };

        return new StatModifier(statType, strategy, duration);
    }
}