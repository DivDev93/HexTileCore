using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using System.Collections.Generic;

public interface IPlayerCard
{
    public PlaceableCard placeable { get; set; }
    public StableDiffusionGenerator imageGenerator { get; set; }
    public List<IBoardSelectablePosition> GetNeighborsFromPlacedTile();
    public EElementType CardType { get; }

}

public class PlayableCard : Entity, IPlayerCard
{
    [Inject]
    ElementalStatModifiersScriptableObject elementalStatModifiers;

    [Inject]
    public CardInfoUI cardInfoUI;

    [Inject]
    IStatModifierFactory statModifierFactory;

    public EElementType CardType => placeable.cardElementType;

    protected override void Awake()
    {
        base.Awake();
        imageGenerator = GetComponentInChildren<StableDiffusionGenerator>();
        placeable = GetComponent<PlaceableCard>();
        imageGenerator.Initialize();
        RefreshStats();
        name = imageGenerator.prompt;
        placeable.OnElementalTileChange += OnElementChange;
        placeable.HighlightOtherPlaceableAction += OnHighlightOtherPlaceable;
    }

    public void OnHighlightOtherPlaceable(IPlaceable otherPlaceable)
    {
        if(otherPlaceable == null)
        {
            CardAttackUI.SetCardsAction?.Invoke(null, null);
            return;
        }

        if (otherPlaceable is PlaceableCard otherCard)
        {
            PlayableCard otherPlayable = otherCard.GetComponent<PlayableCard>();
            CardAttackUI.SetCardsAction?.Invoke(this, otherPlayable);
        }
    }

    void OnDestroy()
    {
        placeable.OnElementalTileChange -= OnElementChange;
        placeable.HighlightOtherPlaceableAction -= OnHighlightOtherPlaceable;
    }

    public PlaceableCard placeable { get; set; }
    public StableDiffusionGenerator imageGenerator { get; set; }

    public void RefreshStats()
    {
        var _monsterType = imageGenerator.words.Find(x => x.wordType == Sentences.WordType.Type).word.ToUpper();
        placeable.cardElementType = (EElementType)System.Enum.Parse(typeof(EElementType), _monsterType);
        baseStats.attack = 0;
        baseStats.defense = 0;
        baseStats.speed = 0;
        foreach (var wordData in imageGenerator.words)
        {
            baseStats.attack += wordData.attackModifier;
            baseStats.defense += wordData.defenseModifier;
            baseStats.speed += wordData.speedModifier;
        }
    }

    public void SetInfoUI()
    {
        cardInfoUI.SetPlayableCard(this);
    }

    public void UnsetInfoUI()
    {
        cardInfoUI.SetPlayableCard(null);
    }

    public void OnElementChange(bool isSameElement)
    {
        //modify stats through stats mediator
        if (isSameElement)
        {
            Stats.Mediator.DisposeAll();
            var attackMod = statModifierFactory.Create(OperatorType.Multiply, EStatType.Attack, elementalStatModifiers.attackMultiplier, -1);
            var defenseMod = statModifierFactory.Create(OperatorType.Multiply, EStatType.Defense, elementalStatModifiers.defenseMultiplier, -1);
            var speedMod = statModifierFactory.Create(OperatorType.Multiply, EStatType.Speed, elementalStatModifiers.speedMultiplier, -1);
            
            Stats.Mediator.AddModifier(attackMod);
            Stats.Mediator.AddModifier(defenseMod);
            Stats.Mediator.AddModifier(speedMod);

            //currentModifiers.Add(attackMod);
            //currentModifiers.Add(defenseMod);
            //currentModifiers.Add(speedMod);
            Debug.Log("Element Change to Same Element update stats");
        }
        else
        {
            Stats.Mediator.DisposeAll();
            Debug.Log("Element Change to Different Element reset stats");
        }
        cardInfoUI.RefreshInfo();
    }

    public List<IBoardSelectablePosition> GetNeighborsFromPlacedTile()
    {
        List<IBoardSelectablePosition> selectedTiles = null;
        placeable.PlacedTarget.SelectNeighbors(Stats.Speed, out selectedTiles);
        return selectedTiles;
    }
}
