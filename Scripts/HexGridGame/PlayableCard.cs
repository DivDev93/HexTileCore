using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;

public interface IPlayerCard
{
    public TilePlaceable placeable { get; set; }
    public StableDiffusionGenerator imageGenerator { get; set; }
    public SentenceGenerator sentenceGenerator { get; set; }
}

public class PlayableCard : MonoBehaviour, IPlayerCard
{
    async void Start()
    {
        imageGenerator = GetComponentInChildren<StableDiffusionGenerator>();
        placeable = GetComponent<TilePlaceable>();        
        sentenceGenerator.OnWordsChosen += RefreshStats;      
    }

    public TilePlaceable placeable { get; set; }
    public StableDiffusionGenerator imageGenerator { get; set; }
    
    [Inject]
    private SentenceGenerator m_sentenceGenerator;
    public SentenceGenerator sentenceGenerator {
        get { return m_sentenceGenerator; }
        set
        {
            m_sentenceGenerator = value;
        }
    }

    public int totalAttack, totalDefense, totalSpeed;

    public void RefreshStats()
    {
        totalAttack = 0;
        totalDefense = 0;
        totalSpeed = 0;
        foreach (var wordData in sentenceGenerator.chosenWords)
        {
            totalAttack += wordData.attackModifier;
            totalDefense += wordData.defenseModifier;
            totalSpeed += wordData.speedModifier;
        }
    }
}
