using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

public interface IPlayerCard
{
    public PlaceableCard placeable { get; set; }
    public StableDiffusionGenerator imageGenerator { get; set; }
}

public class PlayableCard : Entity, IPlayerCard
{
    [Inject]
    public CardInfoUI cardInfoUI;

    public EElementType cardType => placeable.cardElementType;
    void Start()
    {
        imageGenerator = GetComponentInChildren<StableDiffusionGenerator>();
        placeable = GetComponent<PlaceableCard>();
        imageGenerator.Initialize();
        RefreshStats();
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
}
