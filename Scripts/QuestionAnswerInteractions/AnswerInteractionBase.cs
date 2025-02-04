using UnityEngine;
using Unity;

public class AnswerInteractionBase : MonoBehaviour
{
    public QuestionInterationBase questionInteraction;
    public int answerIndex;

    public virtual void SetAnswerOption(QuestionInterationBase _qib, int _answerIndex)
    {
        questionInteraction = _qib;
        answerIndex = _answerIndex;
    }

    public virtual void OnAnswer()
    {
        Debug.Log("Answer Clicked " + questionInteraction.question.options[answerIndex]);
        questionInteraction.ValidateAnswer(answerIndex);
    }
}
