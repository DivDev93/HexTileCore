using PrimeTween;
using TMPro;
using UnityEngine;

public class CardInfoUI : MonoBehaviour
{
    public bool parentToCard = false;
    public float localScale = 0.012f;
    public CanvasGroup canvasGroup;
    PlayableCard currentCard = null;
    public PlayableCard CurrentCard
    {
        get => currentCard;
        set
        {
            currentCard = value;
            if (value == null)
            {
                SetWindowOpened(false);
                return;
            }

            if (parentToCard)
            {
                transform.SetParent(CurrentCard.transform);

                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = localScale * Vector3.one;
            }
            else
            {
                transform.SetParent(null);
            }

            RefreshInfo();
            SetWindowOpened(true);
            //Debug.Log("Card Info Set should have updated UI text with mediator count  " + value.Stats.Mediator.MediatorCount);
        }
    }

    public TextMeshProUGUI cardType;
    public TextMeshProUGUI attackValue;
    public TextMeshProUGUI defenseValue;
    public TextMeshProUGUI speedValue;

    public void RefreshInfo()
    {
        if (CurrentCard == null)
        {
            return;
        }

        cardType.text = CurrentCard.CardType.ToString();
        attackValue.text = CurrentCard.Stats.Attack.ToString();
        defenseValue.text = CurrentCard.Stats.Defense.ToString();
        speedValue.text = CurrentCard.Stats.Speed.ToString();
    }

    public void SetWindowOpened(bool isOpened)
    {
        if(!isOpened)
        {
            currentCard = null;
        }

        Tween.Custom(canvasGroup.alpha, isOpened ? 1 : 0, 0.5f, (value) => canvasGroup.alpha = value); 
    }

    public void SetPlayableCard(PlayableCard card)
    {
        CurrentCard = card;
    }
}
