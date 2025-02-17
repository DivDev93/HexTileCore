public enum EStatType { Attack, Defense, Speed }

public class Stats {
    readonly StatsMediator mediator;
    readonly BaseStats baseStats;
    
    public StatsMediator Mediator => mediator;
    
    public int Attack {
        get {
            var q = new Query(EStatType.Attack, baseStats.attack);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }
    
    public int Defense {
        get {
            var q = new Query(EStatType.Defense, baseStats.defense);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }

    public int Speed
    {
        get
        {
            var q = new Query(EStatType.Speed, baseStats.speed);
            mediator.PerformQuery(this, q);
            return q.Value;
        }
    }


    public Stats(StatsMediator mediator, BaseStats baseStats) {
        this.mediator = mediator;
        this.baseStats = baseStats;
    }
    
    public override string ToString() => $"Attack: {Attack}, Defense: {Defense}, Speed: {Speed}";
}