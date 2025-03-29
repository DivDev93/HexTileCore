using UnityEngine;

//namespace DesignPatterns.Command
//{
// interface to wrap your actions in a "command object"
public interface ICommand
{
    public void Execute();
    public void Undo();
}

public class PlaceOnBoardCommand : ICommand
{
    IBoardSelectablePosition newGripPos;
    IBoardSelectablePosition oldGripPos;
    public BoardPlaceable card;

    public PlaceOnBoardCommand(BoardPlaceable card, IBoardSelectablePosition highlighted, IBoardSelectablePosition placed)
    {          
        this.newGripPos = highlighted;
        this.card = card;
        this.oldGripPos = placed;     

        Debug.Log("Created command with Old Grip Pos: " + oldGripPos + " new grip pos " + newGripPos);

    }

    public void Execute()
    {
        card.PlaceOnTile(newGripPos.GridPosition);
    }

    public void Undo()
    {
        if (oldGripPos != null)
        {
            Debug.Log("Undoing Command to old tile " + oldGripPos);
            card.PlaceOnTile(oldGripPos.GridPosition);
        }
        else
        {
            card.OnSelected();
            card.TweenUp();
            card.HighlightedTarget = card.PlacedTarget = null;
            Debug.Log("Undoing Command to null should have tweened up ");
        }
    }
}

public class AttackCardCommand : ICommand
{
    StatModifier defenseModifier;
    CardInfoUI attackingCard;
    CardInfoUI defendingCard;
    IStatModifierFactory statModifierFactory;

    public AttackCardCommand(CardInfoUI attackingCard, CardInfoUI defendingCard, IStatModifierFactory statModifierFactory)
    {
        this.attackingCard = attackingCard;
        this.defendingCard = defendingCard;
        this.statModifierFactory = statModifierFactory;
    }

    public void Execute()
    {
        var attackerCard = attackingCard.CurrentCard;
        var defenderCard = defendingCard.CurrentCard;
        var attackValue = attackerCard.Stats.Attack;
        var defenseValue = defenderCard.Stats.Defense;
        var damage = attackValue - defenseValue;
        if (damage > 0)
        {
            IOperationStrategy attackStrategy = new AddOperation(-damage);

            defenseModifier = statModifierFactory.Create(OperatorType.Add, EStatType.Defense, -damage);
            defenderCard.Stats.Mediator.AddModifier(defenseModifier);
            defendingCard.RefreshInfo();
        }
    }

    public void Undo()
    {
        defenseModifier.MarkedForRemoval = true;
        defendingCard.RefreshInfo();
    }
}
//}

