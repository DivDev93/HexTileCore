using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

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

            cardType.text = value.cardType.ToString();
            attackValue.text = value.Stats.Attack.ToString();
            defenseValue.text = value.Stats.Defense.ToString();
            speedValue.text = value.Stats.Speed.ToString();
            SetWindowOpened(true);
        }
    }

    public TextMeshProUGUI cardType;
    public TextMeshProUGUI attackValue;
    public TextMeshProUGUI defenseValue;
    public TextMeshProUGUI speedValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void SetWindowOpened(bool isOpened)
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
