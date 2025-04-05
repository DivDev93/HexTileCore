using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using System.Collections.Generic;

public interface IPlayerCard
{
    PlaceableCard placeable { get; }
    StableDiffusionGenerator imageGenerator { get; }
    List<IBoardSelectablePosition> GetNeighborsFromPlacedTile();
    EElementType CardType { get; }
    void RefreshStats();
    void SetInfoUI();
    void UnsetInfoUI();
}

public class PlayableCard : Entity, IPlayerCard
{
    [Inject] private ElementalStatModifiersScriptableObject elementalStatModifiers;
    [Inject] private CardInfoUI cardInfoUI;
    [Inject] private IStatModifierFactory statModifierFactory;

    private PlaceableCard _placeable;
    private StableDiffusionGenerator _imageGenerator;
    private List<StatModifier> currentModifiers = new List<StatModifier>();

    public PlaceableCard placeable => _placeable;
    public StableDiffusionGenerator imageGenerator => _imageGenerator;
    public EElementType CardType => placeable.cardElementType;

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
        SetupEventHandlers();
    }

    private void InitializeComponents()
    {
        _imageGenerator = GetComponentInChildren<StableDiffusionGenerator>();
        _placeable = GetComponent<PlaceableCard>();
        _imageGenerator.Initialize();
        
        RefreshStats();
        name = _imageGenerator.prompt;
    }

    private void SetupEventHandlers()
    {
        if (_placeable != null)
        {
            _placeable.OnElementalTileChange += OnElementChange;
            _placeable.HighlightOtherPlaceableAction += OnHighlightOtherPlaceable;
        }
    }

    private void OnDestroy()
    {
        if (_placeable != null)
        {
            _placeable.OnElementalTileChange -= OnElementChange;
            _placeable.HighlightOtherPlaceableAction -= OnHighlightOtherPlaceable;
        }
    }

    public void RefreshStats()
    {
        UpdateElementType();
        ResetBaseStats();
        ApplyWordModifiers();
    }

    private void UpdateElementType()
    {
        var monsterType = _imageGenerator.words.Find(x => x.wordType == Sentences.WordType.Type).word.ToUpper();
        if (!string.IsNullOrEmpty(monsterType))
        {
            _placeable.cardElementType = (EElementType)System.Enum.Parse(typeof(EElementType), monsterType);
        }
    }

    private void ResetBaseStats()
    {
        baseStats.attack = 0;
        baseStats.defense = 0;
        baseStats.speed = 0;
    }

    private void ApplyWordModifiers()
    {
        foreach (var wordData in _imageGenerator.words)
        {
            baseStats.attack += wordData.attackModifier;
            baseStats.defense += wordData.defenseModifier;
            baseStats.speed += wordData.speedModifier;
        }
    }

    public void OnHighlightOtherPlaceable(IPlaceable otherPlaceable)
    {
        if (otherPlaceable == null)
        {
            CardAttackUI.SetCardsAction?.Invoke(null, null);
            return;
        }

        if (otherPlaceable is PlaceableCard otherCard)
        {
            var otherPlayable = otherCard.GetComponent<PlayableCard>();
            CardAttackUI.SetCardsAction?.Invoke(this, otherPlayable);
        }
    }

    public void OnElementChange(bool isSameElement)
    {
        Stats.Mediator.DisposeAll();
        
        if (isSameElement)
        {
            ApplyElementalBonuses();
        }

        cardInfoUI.RefreshInfo();
    }

    private void ApplyElementalBonuses()
    {
        var attackMod = statModifierFactory.Create(OperatorType.Multiply, EStatType.Attack, elementalStatModifiers.attackMultiplier);
        var defenseMod = statModifierFactory.Create(OperatorType.Multiply, EStatType.Defense, elementalStatModifiers.defenseMultiplier);
        var speedMod = statModifierFactory.Create(OperatorType.Multiply, EStatType.Speed, elementalStatModifiers.speedMultiplier);
        
        Stats.Mediator.AddModifier(attackMod);
        Stats.Mediator.AddModifier(defenseMod);
        Stats.Mediator.AddModifier(speedMod);
    }

    public List<IBoardSelectablePosition> GetNeighborsFromPlacedTile()
    {
        if (_placeable?.PlacedTarget == null) return null;
        
        List<IBoardSelectablePosition> selectedTiles = null;
        _placeable.PlacedTarget.SelectNeighbors(Stats.Speed, out selectedTiles);
        return selectedTiles;
    }

    public void SetInfoUI()
    {
        cardInfoUI?.SetPlayableCard(this);
    }

    public void UnsetInfoUI()
    {
        cardInfoUI?.SetPlayableCard(null);
    }
}
