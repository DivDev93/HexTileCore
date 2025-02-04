using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestionInterationBase : MonoBehaviour
{
    public TriviaQuestion question;
    public List<AnswerInteractionBase> answerInteractions;

    public void SetQuestion(TriviaQuestion _question)
    {
        this.question = _question;
        for (int i = 0; i < answerInteractions.Count; i++)
        {
            answerInteractions[i].SetAnswerOption(this, i);
        }
    }

    public bool CheckAnswer(int _answerIndex)
    {
        return question.correctAnswer == question.options[_answerIndex];
    }

    public void ValidateAnswer(int _answerIndex)
    {
        if (CheckAnswer(_answerIndex))
        {
            OnCorrectAnswer();
        }
        else
        {
            OnWrongAnswer();
        }
    }

    public virtual void OnCorrectAnswer()
    {
        QuestionManager.OnQuestionAnswered?.Invoke(question, true);
        //Debug.Log("Correct Answer");
    }

    public virtual void OnWrongAnswer()
    {
        QuestionManager.OnQuestionAnswered?.Invoke(question, false);
        //Debug.Log("Wrong Answer");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        answerInteractions = new List<AnswerInteractionBase>(GetComponentsInChildren<AnswerInteractionBase>());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGUI()
    {
        //button handlers for each answer
        for (int i = 0; i < answerInteractions.Count; i++)
        {
            if (GUI.Button(new Rect(300, 10 + (i * 30), 200, 20), question.options[i]))
            {
                answerInteractions[i].OnAnswer();
            }
        }
    }
}
