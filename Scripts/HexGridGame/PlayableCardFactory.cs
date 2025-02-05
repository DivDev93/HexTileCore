using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityUtils;

public class PlayableCardFactory : MonoBehaviour
{
    public PlayableCard cardPrefab;

    public IPlayerCard CreateCard(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var card = Instantiate(cardPrefab, position, rotation);
        card.placeable.transform.localScale = scale;
        return card;
    }
}
