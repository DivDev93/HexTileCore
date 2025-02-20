using UnityEngine;

public abstract class Entity : MonoBehaviour, IVisitable {
    [SerializeField] protected BaseStats baseStats;
    public Stats Stats { get; private set; }

    protected virtual void Awake() {
        if (baseStats == null)
        {
            baseStats = new BaseStats();
        }
        Stats = new Stats(new StatsMediator(), baseStats);
    }

    public void Update() {
        Stats.Mediator.Update(Time.deltaTime);
    }
    
    public void Accept(IVisitor visitor) => visitor.Visit(this);
}