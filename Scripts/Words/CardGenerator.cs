using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using Sentences;

public class CardGenerator
{
    private static CardGenerator _instance;
    public static CardGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CardGenerator();
            }
            return _instance;
        }
    }
    
    public ObjectPool<MonsterCard> monsterCardPool;
    MonsterCard cardPrefab;

    public CardGenerator()
    {
        cardPrefab = Resources.Load<MonsterCard>("MonsterCard");
        monsterCardPool = new ObjectPool<MonsterCard>(CreatePooledObject,
            OnGetFromPool,
            ReturnToPool,
            OnDestroyPooledCard,
            maxSize: 20);
    }

    public MonsterCard CreatePooledObject()
    {
        return UnityEngine.Object.Instantiate(cardPrefab);
    }

    public void OnGetFromPool(MonsterCard card)
    {
        card.gameObject.SetActive(true);
    }
    public void ReturnToPool(MonsterCard card)
    {
        card.gameObject.SetActive(false);
    }

    public void OnDestroyPooledCard(MonsterCard card)
    {
        UnityEngine.Object.Destroy(card.gameObject);
    }

    public void ReleaseCardData(MonsterCard cardData)
    {
        monsterCardPool.Release(cardData);
    }    

    public MonsterCard GenerateCard(
    SentenceTemplate selectedTemplate,
    List<WordData> chosenWords,
    int baseAttack = 0,
    int baseDefense = 0,
    int baseSpeed = 0
)
    {
        // Build final sentence and calculate stats
        int totalAttack = baseAttack;
        int totalDefense = baseDefense;
        int totalSpeed = baseSpeed;
        int totalCost = 0;

        // Replace placeholders in the template
        string finalSentence = selectedTemplate.templateText;

        for (int i = 0; i < chosenWords.Count; i++)
        {
            var wordInfo = chosenWords[i];
            totalAttack += wordInfo.attackModifier;
            totalDefense += wordInfo.defenseModifier;
            totalSpeed += wordInfo.speedModifier;
            totalCost += wordInfo.cost;

            // For this example, assume placeholders are in brackets in order, e.g. [ADJECTIVE], [NOUN], etc.
            string placeholder = "[" + selectedTemplate.slotSequence[i].ToString().ToUpper() + "]";
            finalSentence = finalSentence.Replace(placeholder, wordInfo.word);
        }

        // Check if totalCost exceeds allowed points
        if (totalCost > 10)
        {
            // Handle scenario: either reject the card, or trim stats, or prompt the user to replace some words
        }

        // Construct the Card object with final stats
        //CardData newCard = 

        var monsterCard = monsterCardPool.Get();
        monsterCard.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1;
        monsterCard.SetCardData(new()
        {
            name = "Generated Card",
            description = finalSentence,
            attack = totalAttack,
            defense = totalDefense,
            speed = totalSpeed,
            cost = totalCost
        });

        return monsterCard;
    }

}
