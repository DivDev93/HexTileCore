using System.Collections;
using System.Collections.Generic;
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
    BoardPlaceable card;

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
            Debug.Log("Undoing Command to null should have tweened up ");
        }
    }
}
//}

