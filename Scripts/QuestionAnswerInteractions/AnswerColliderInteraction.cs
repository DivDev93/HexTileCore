using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent (typeof(Rigidbody), typeof(Collider))]
public class AnswerColliderInteraction : AnswerInteractionBase, ISelectHandler
{
    Rigidbody rb;
    StableDiffusionGenerator sdg;

    public void OnSelect(BaseEventData eventData)
    {
        OnAnswer();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sdg = GetComponent<StableDiffusionGenerator>();
        rb.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.CompareTag("Projectile"))
        {
            Debug.Log("Answer Clicked " + collision.gameObject.name);
            OnAnswer();
        }
    }

    public override void SetAnswerOption(QuestionInterationBase _qib, int _answerIndex)
    {
        base.SetAnswerOption(_qib, _answerIndex);
        sdg.SetPrompt(_qib.question.options[_answerIndex]);
        sdg.StartImageGeneration();
    }

    public override void OnAnswer()
    {
        rb.isKinematic = false;
        base.OnAnswer();
    }
}
