using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

public interface IPlayerCard
{
    public PlaceableCard placeable { get; set; }
    public StableDiffusionGenerator imageGenerator { get; set; }
}

public class PlayableCard : MonoBehaviour, IPlayerCard
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

    public int totalAttack, totalDefense, totalSpeed;

    public void RefreshStats()
    {
        var _monsterType = imageGenerator.words.Find(x => x.wordType == Sentences.WordType.Type).word.ToUpper();
        placeable.cardElementType = (EElementType)System.Enum.Parse(typeof(EElementType), _monsterType);
        totalAttack = 0;
        totalDefense = 0;
        totalSpeed = 0;
        foreach (var wordData in imageGenerator.words)
        {
            totalAttack += wordData.attackModifier;
            totalDefense += wordData.defenseModifier;
            totalSpeed += wordData.speedModifier;
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
