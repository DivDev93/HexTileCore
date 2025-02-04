using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SentenceDataLoader : MonoBehaviour
{
    public string jsonFileName = "SentenceData.json";
    public SentenceData loadedData;
    public bool validate = false;

    public void OnValidate()
    {
        if (validate)
        {
            LoadSentenceData();
        }

    }
    public void LoadSentenceData()
    {
        string filePath = Path.Combine(Application.dataPath, jsonFileName);

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);

            // Deserialize into an intermediate wrapper class
            SentenceDataWrapper wrapper = JsonUtility.FromJson<SentenceDataWrapper>(jsonContent);

            // Convert intermediate class to the actual SentenceData
            wrapper.ToSentenceData(ref loadedData);
            Debug.Log("SentenceData loaded successfully.");
        }
        else
        {
            Debug.LogError($"JSON file not found at {filePath}");
        }
    }

    [System.Serializable]
    private class SentenceDataWrapper
    {
        public List<SentenceTemplateWrapper> sentenceTemplates;
        public List<WordOptionWrapper> words;

        public SentenceData ToSentenceData(ref SentenceData data)
        {
            //SentenceData data = ScriptableObject.CreateInstance<SentenceData>();

            // Convert SentenceTemplateWrapper to SentenceTemplate
            data.sentenceTemplates = new List<SentenceTemplate>();
            foreach (var templateWrapper in sentenceTemplates)
            {
                SentenceTemplate template = new SentenceTemplate
                {
                    templateText = templateWrapper.templateText,
                    slotSequence = new List<WordType>()
                };

                foreach (var slot in templateWrapper.slotSequence)
                {
                    if (System.Enum.TryParse(slot, out WordType wordType))
                    {
                        template.slotSequence.Add(wordType);
                    }
                    else
                    {
                        Debug.LogError($"Invalid WordType: {slot}");
                    }
                }

                data.sentenceTemplates.Add(template);
            }

            // Convert WordOptionWrapper to WordOption
            data.words = new List<WordOption>();
            foreach (var wordOptionWrapper in words)
            {
                WordOption wordOption = new WordOption
                {
                    wordType = (WordType)System.Enum.Parse(typeof(WordType), wordOptionWrapper.wordType),
                    possibleWords = new List<WordData>()
                };

                foreach (var wordDataWrapper in wordOptionWrapper.possibleWords)
                {
                    wordOption.possibleWords.Add(new WordData
                    {
                        wordType = (WordType)System.Enum.Parse(typeof(WordType), wordDataWrapper.wordType),
                        word = wordDataWrapper.word,
                        cost = wordDataWrapper.cost,
                        attackModifier = wordDataWrapper.attackModifier,
                        defenseModifier = wordDataWrapper.defenseModifier,
                        speedModifier = wordDataWrapper.speedModifier
                    });
                }

                data.words.Add(wordOption);
            }

            return data;
        }
    }

    [System.Serializable]
    private class SentenceTemplateWrapper
    {
        public string templateText;
        public List<string> slotSequence; // Keep as strings for initial deserialization
    }

    [System.Serializable]
    private class WordOptionWrapper
    {
        public string wordType;
        public List<WordDataWrapper> possibleWords;
    }

    [System.Serializable]
    private class WordDataWrapper
    {
        public string wordType;
        public string word;
        public int cost;
        public int attackModifier;
        public int defenseModifier;
        public int speedModifier;
    }
}
