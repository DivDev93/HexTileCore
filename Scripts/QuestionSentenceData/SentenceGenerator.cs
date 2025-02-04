using System.Collections.Generic;
using UnityEngine;

public class SentenceGenerator : MonoBehaviour
{
    public SentenceData sentenceData;
    public int wordOptionsCount = 8;

    public TMPro.TextMeshProUGUI sentenceText;
    public List<SentenceTemplate> sentenceTemplates => sentenceData.sentenceTemplates;
    public RouletteWheel rouletteWheel;
    private SentenceTemplate selectedTemplate;
    List<WordData> options = new();
    private List<WordData> chosenWords = new();
    private int currentSlotIndex = 0;

    private void Start()
    {
        rouletteWheel.OnRouletteWheelStopped += OnRouletteWheelStopped;
        SelectRandomTemplate();
        if(sentenceText != null)
            sentenceText.text = "";
    }

    public void SelectRandomTemplate()
    {
        int randomIndex = Random.Range(0, sentenceTemplates.Count);
        selectedTemplate = sentenceTemplates[randomIndex];
        currentSlotIndex = 0;
        chosenWords.Clear();
        Debug.Log("Selected Template: " + selectedTemplate.templateText);
        PopulateRouletteWheel();
    }

    bool IsRouletteWheelDisabled()
    {
        return !rouletteWheel.enabled || !rouletteWheel.gameObject.activeSelf;
    }

    private void PopulateRouletteWheel()
    {
        if (IsRouletteWheelDisabled())
        {
            Debug.Log("Roulette wheel is disabled.");
            return;
        }

        if (currentSlotIndex >= selectedTemplate.slotSequence.Count)
        {
            Debug.LogError("All slots in the template are already filled.");
            return;
        }

        WordType currentWordType = selectedTemplate.slotSequence[currentSlotIndex];
        List<WordData> options = FindWordOptions(currentWordType, wordOptionsCount);

        rouletteWheel.labelList.ForEach(label => rouletteWheel.labelPool.Release(label));
        rouletteWheel.labelList.Clear();

        for (int i = 0; i < options.Count; i++)
        {
            var label = rouletteWheel.GetRouletteLabel(options[i].word, i * rouletteWheel.spacing);
            rouletteWheel.labelList.Add(label);
        }
    }

    private List<WordData> FindWordOptions(WordType wordType, int count)
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

    public void OnRouletteWheelStopped(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= rouletteWheel.labelList.Count)
        {
            Debug.LogError("Invalid index from roulette wheel.");
            return;
        }

        WordData selectedWord = options[selectedIndex];
        chosenWords.Add(selectedWord);
        Debug.Log("Chosen Word: " + selectedWord.word);

        currentSlotIndex++;
        if (currentSlotIndex >= selectedTemplate.slotSequence.Count)
        {
            FinalizeSentence(true);
        }
        else
        {
            FinalizeSentence();
            PopulateRouletteWheel();
        }
    }

    string InsertWordsToTemplate(SentenceTemplate template, List<WordData> words)
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

    private void FinalizeSentence(bool generateCard = false)
    {
        string finalSentence = InsertWordsToTemplate(selectedTemplate, chosenWords);
        //    selectedTemplate.templateText;
        //for (int i = 0; i < chosenWords.Count; i++)
        //{
        //    string placeholder = "[" + selectedTemplate.slotSequence[i].ToString().ToUpper() + "]";
        //    int placeholderIndex = finalSentence.IndexOf(placeholder);
        //    if (placeholderIndex != -1)
        //    {
        //        finalSentence = finalSentence.Remove(placeholderIndex, placeholder.Length).Insert(placeholderIndex, chosenWords[i].word);
        //    }
        //}

        Debug.Log("Final Sentence: " + finalSentence);
        sentenceText.text = finalSentence;

        if (generateCard)
        {
            // Generate the card  
            CardGenerator.Instance.GenerateCard(selectedTemplate, chosenWords);
            rouletteWheel.gameObject.SetActive(false);
        }
    }

    public string GetRandomizedPrompt()
    {    
        var template = sentenceTemplates[Random.Range(0, sentenceTemplates.Count - 1)];
        string prompt = template.templateText;
        Debug.Log("Selected Template: " + template.templateText);
        List<WordData> possibleWords = new List<WordData>();
        chosenWords.Clear();
        for (int i = 0; i < template.slotSequence.Count; i++)
        {
            WordType wordType = template.slotSequence[i];
            possibleWords = sentenceData.words.Find(x => x.wordType == wordType).possibleWords;
            int randomIndex = Random.Range(0, possibleWords.Count);
            chosenWords.Add(possibleWords[randomIndex]);
        }
        return InsertWordsToTemplate(template, chosenWords);
        //return prompt;
    }
}
