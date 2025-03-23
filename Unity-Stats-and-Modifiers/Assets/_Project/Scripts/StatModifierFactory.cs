using System;

public interface IStatModifierFactory {
    StatModifier Create(OperatorType operatorType, EStatType statType, int value, float duration = -1);
    StatModifier Create(OperatorType operatorType, EStatType statType, float value, float duration = -1);

}

public class StatModifierFactory : IStatModifierFactory {
    public StatModifier Create(OperatorType operatorType, EStatType statType, int value, float duration = -1) {
        IOperationStrategy strategy = operatorType switch {
            OperatorType.Add => new AddOperation(value),
            OperatorType.Multiply => new MultiplyOperation(value),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        return new StatModifier(statType, strategy, duration);
    }

    public StatModifier Create(OperatorType operatorType, EStatType statType, float value, float duration = -1)
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