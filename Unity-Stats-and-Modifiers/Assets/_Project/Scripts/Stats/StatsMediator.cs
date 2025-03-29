using System.Collections.Generic;
using System.Linq;

public class StatsMediator {
    readonly List<StatModifier> listModifiers = new();
    readonly Dictionary<EStatType, IEnumerable<StatModifier>> modifiersCache = new();
    readonly IStatModifierApplicationOrder order = new NormalStatModifierOrder(); // OR INJECT
    public int MediatorCount => listModifiers.Count;

    public void PerformQuery(object sender, Query query) {
        if (!modifiersCache.ContainsKey(query.StatType)) {
            modifiersCache[query.StatType] = listModifiers.Where(modifier => modifier.Type == query.StatType).ToList();
        }
        query.Value = order.Apply(modifiersCache[query.StatType], query.Value);
    }

    void InvalidateCache(EStatType statType) {
        modifiersCache.Remove(statType);
    }

    public void AddModifier(StatModifier modifier) {
        listModifiers.Add(modifier);
        InvalidateCache(modifier.Type);
        modifier.MarkedForRemoval = false;
        
        modifier.OnDispose += _ => InvalidateCache(modifier.Type);
        modifier.OnDispose += _ => listModifiers.Remove(modifier);
    }

    public void DisposeAll()
    {
        foreach (var modifier in listModifiers)
        {
            modifier.MarkedForRemoval = true;
        }
    }

    public void Update(float deltaTime) {
        foreach (var modifier in listModifiers) {
            modifier.Update(deltaTime);
        }
        
        foreach (var modifier in listModifiers.Where(modifier => modifier.MarkedForRemoval).ToList()) {
            modifier.Dispose();
        }
    }
}

public class Query {
    public readonly EStatType StatType;
    public int Value;

    public Query(EStatType statType, int value) {
        StatType = statType;
        Value = value;
    }
}
