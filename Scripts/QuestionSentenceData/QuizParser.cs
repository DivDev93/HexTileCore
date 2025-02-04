using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class TriviaQuestion
{
    public string question;
    public List<string> options;
    public string correctAnswer;
}

[System.Serializable]
public class TriviaQuestionList
{
    public List<TriviaQuestion> questions;
}

public class QuizParser : MonoBehaviour
{
    string quizJson;
    public TriviaQuestionList questions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        quizJson = File.ReadAllText(Application.dataPath + "/quiz.json");
        questions = JsonUtility.FromJson<TriviaQuestionList>(quizJson);
        QuestionManager.OnQuestionsGenerated(questions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
