using UnityEngine;

public class MonsterCard : MonoBehaviour
{
    public CardData cardData;
    public TMPro.TextMeshProUGUI sentenceText;
    public StableDiffusionGenerator sdg;

    public void SetCardData(CardData cardData)
    {
        this.cardData = cardData;
        sentenceText.text = cardData.description;
        sdg.SetPrompt(cardData.description);
        sdg.StartImageGeneration();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
