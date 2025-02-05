using System.Collections.Generic;
using UnityEngine;
using Sentences;
using Reflex.Attributes;

public class SentenceGenerator : MonoBehaviour
{
    [Inject]
    SentenceData sentenceData;
    public int wordOptionsCount = 8;

    public TMPro.TextMeshProUGUI sentenceText;
    public List<SentenceTemplate> sentenceTemplates => sentenceData.sentenceTemplates;
    protected SentenceTemplate selectedTemplate;
    protected List<WordData> options = new();
    public List<WordData> chosenWords = new();
    public System.Action OnWordsChosen;
    protected int currentSlotIndex = 0;

    protected virtual void Awake()
    {
        SelectRandomTemplate();
        if(sentenceText != null)
            sentenceText.text = "";
    }

    public virtual void SelectRandomTemplate()
    {
        int randomIndex = Random.Range(0, sentenceTemplates.Count);
        selectedTemplate = sentenceTemplates[randomIndex];
        currentSlotIndex = 0;
        chosenWords.Clear();
        Debug.Log("Selected Template: " + selectedTemplate.templateText);
    }

    protected List<WordData> FindWordOptions(WordType wordType, int count)
    {
        //select random words from the list of possible words
        options.Clear();
        List<WordData> possibleWords = sentenceData.words.Find(x => x.wordType == wordType).possibleWords;
        for (int i = 0; i < count; i++)
        {
            int randomIndex;
            bool found = false;
            do
            {
                randomIndex = Random.Range(0, possibleWords.Count);
                Debug.Log("Random Index: " + randomIndex + " label is " + possibleWords[randomIndex].word);
                if (!options.Contains(possibleWords[randomIndex]))
                {
                    options.Add(possibleWords[randomIndex]);
                    found = true;
                }

            } while (!found);
        }

        return options;
    }

    protected string InsertWordsToTemplate(SentenceTemplate template, List<WordData> words)
    {
        string finalSentence = template.templateText;
        Debug.Log("Inserting words to template: " + finalSentence + " with words: " + words.Count + " and slot sequence: " + template.slotSequence.Count);
        for (int i = 0; i < words.Count; i++)
        {
            string placeholder = "[" + template.slotSequence[i].ToString().ToUpper() + "]";
            int placeholderIndex = finalSentence.IndexOf(placeholder);
            if (placeholderIndex != -1)
            {
                finalSentence = finalSentence.Remove(placeholderIndex, placeholder.Length).Insert(placeholderIndex, words[i].word);
            }
        }
        return finalSentence;
    }

    int RandomIndex(int count)
    {
        System.Random random = new System.Random(Time.frameCount);
        return random.Next(0, count);
    }

    public string GetRandomizedPrompt()
    {
        var template = sentenceTemplates[RandomIndex(sentenceTemplates.Count - 1)];
        GetRandomizedWordDatas(template);
        OnWordsChosen?.Invoke();
        return InsertWordsToTemplate(template, chosenWords);
        //return prompt;
    }

    public List<WordData> GetRandomizedWordDatas(SentenceTemplate template)
    {
        string prompt = template.templateText;
        Debug.Log("Selected Template: " + template.templateText);
        List<WordData> possibleWords = new List<WordData>();
        chosenWords.Clear();
        for (int i = 0; i < template.slotSequence.Count; i++)
        {
            WordType wordType = template.slotSequence[i];
            possibleWords = sentenceData.words.Find(x => x.wordType == wordType).possibleWords;
            int randomIndex = RandomIndex(possibleWords.Count);
            chosenWords.Add(possibleWords[randomIndex]);
        }
        OnWordsChosen?.Invoke();
        return chosenWords;
        //return prompt;
    }
}
