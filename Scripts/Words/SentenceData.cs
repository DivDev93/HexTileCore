using System.Collections.Generic;
using UnityEngine;

namespace Sentences
{
    [System.Serializable]
    public struct SentenceTemplate
    {
        public string templateText; // e.g. "A [ADJECTIVE] [NOUN] [VERB] [PREPOSITION] a [NOUN]."
        public List<WordType> slotSequence; // e.g. [Adjective, Noun, Verb, Preposition, Noun]
    }

    [System.Serializable]
    public struct WordData
    {
        public WordType wordType;
        public string word;
        public int cost;
        public int attackModifier;
        public int defenseModifier;
        public int speedModifier;
    }

    public enum WordType
    {
        Type,
        Adjective,
        MainNoun,
        Verb,
        Adverb,
        Preposition,
        SecondaryNoun
    }

    [System.Serializable]
    public struct WordOption
    {
        public WordType wordType;
        public List<WordData> possibleWords;
    }

    [System.Serializable]
    public class CardData
    {
        public string name;
        public string description;
        public int attack;
        public int defense;
        public int speed;
        public int cost;
    }

    //Scriptable Object to hold the list of SentenceTemplates and WordDatas
    [CreateAssetMenu(fileName = "SentenceData", menuName = "ScriptableObjects/SentenceData", order = 1)]
    public class SentenceData : ScriptableObject
    {
        public List<SentenceTemplate> sentenceTemplates;
        public List<WordOption> words;
    }
}