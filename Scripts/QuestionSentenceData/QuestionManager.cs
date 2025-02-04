using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestionManager : MonoBehaviour
{
    public static Action<TriviaQuestionList> OnQuestionsGenerated;
    public static Action<TriviaQuestion, bool> OnQuestionAnswered;

    public int currentQuestionIndex = -1;
    public TriviaQuestionList triviaQuestions;
    public TriviaQuestion CurrentQuestion => triviaQuestions.questions[currentQuestionIndex];

    public QuestionInterationBase questionInterationBase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        OnQuestionsGenerated += QuestionGenerated;
        OnQuestionAnswered += QuestionAnswered;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void QuestionGenerated(TriviaQuestionList questions)
    {
        triviaQuestions = questions;
        NextQuestion();
    }

    public void QuestionAnswered(TriviaQuestion question, bool isCorrect)
    {
        if (isCorrect)
        {
            Debug.Log("Correct Answer");
        }
        else
        {
            Debug.Log("Wrong Answer");
        }
        NextQuestion();
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex >= triviaQuestions.questions.Count)
        {
            currentQuestionIndex = 0;
        }
        questionInterationBase.SetQuestion(CurrentQuestion);
    }
}
