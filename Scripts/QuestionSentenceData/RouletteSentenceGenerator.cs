using System.Collections.Generic;
using UnityEngine;
using Sentences;

public class RouletteSentenceGenerator : SentenceGenerator
{
    public RouletteWheel rouletteWheel;

    protected override void Awake()
    {
        rouletteWheel.OnRouletteWheelStopped += OnRouletteWheelStopped;
        base.Awake();
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

    public override void SelectRandomTemplate()
    {
        base.SelectRandomTemplate();
        PopulateRouletteWheel();
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
            OnWordsChosen?.Invoke();
        }
    }
}
