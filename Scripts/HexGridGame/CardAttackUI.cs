using System;
using NUnit.Framework;
using Reflex.Attributes;
using UnityEngine;

public class CardAttackUI : MonoBehaviour
{
    [Inject]
    IStatModifierFactory statModifierFactory;

    public CardInfoUI attacker;
    public CardInfoUI defender;

#pragma warning disable UDR0001 // Domain Reload Analyzer
    public static Action<PlayableCard, PlayableCard> SetCardsAction;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    public void Awake()
    {
        SetCardsAction += SetCards;
    }

    public void OnDestroy()
    {
        SetCardsAction -= SetCards;
    }

    public void SetCards(PlayableCard attackerCard, PlayableCard defenderCard)
    {
        if(attackerCard == null || defenderCard == null)
        {
            attacker.SetWindowOpened(false);
            return;
        }
        attacker.CurrentCard = attackerCard;
        defender.CurrentCard = defenderCard;
        attacker.SetWindowOpened(true);
        Attack();
    }

    public void Attack()
    {
        var command = new AttackCardCommand(attacker, defender, statModifierFactory);
        attacker.CurrentCard.placeable.player.Commands.ExecuteCommand(command);

        //if (attacker.CurrentCard == null || defender.CurrentCard == null)
        //{
        //    return;
        //}
        //var attackerCard = attacker.CurrentCard;
        //var defenderCard = defender.CurrentCard;
        //var attackValue = attackerCard.Stats.Attack;
        //var defenseValue = defenderCard.Stats.Defense;
        //var damage = attackValue - defenseValue;
        //if (damage > 0)
        //{
        //    IOperationStrategy attackStrategy = new AddOperation(-damage);           

        //    StatModifier defenseModifier = statModifierFactory.Create(OperatorType.Add, EStatType.Defense, -damage);
        //    defenderCard.Stats.Mediator.AddModifier(defenseModifier);
        //    defender.RefreshInfo();
        //}
    }

    public void Undo()
    {
        attacker.CurrentCard.placeable.player.Commands.UndoCommand();
        attacker.SetWindowOpened(false);
    }
}
