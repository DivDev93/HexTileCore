using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CommandInvoker
{
    // Stack of command objects to undo
    private Stack<ICommand> s_UndoStack = new Stack<ICommand>();

    // Second stack of redoable commands
    private Stack<ICommand> s_RedoStack = new Stack<ICommand>();

    // Execute a command object directly and save to the undo stack
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        s_UndoStack.Push(command);

        // Clear out the redo stack if we make a new move
        s_RedoStack.Clear();
    }

    public ICommand GetLastCommand()
    {
        if (s_UndoStack.Count > 0)
        {
            ICommand activeCommand = s_UndoStack.Peek();
            return activeCommand;
        }
        return null;
    }

    public void UndoCommand()
    {
        // If we have commands to undo
        if (s_UndoStack.Count > 0)
        {
            ICommand activeCommand = s_UndoStack.Pop();
            s_RedoStack.Push(activeCommand);
            activeCommand.Undo();
        }
    }

    public void RedoCommand()
    {
        // If we have commands to redo
        if (s_RedoStack.Count > 0)
        {
            ICommand activeCommand = s_RedoStack.Pop();
            s_UndoStack.Push(activeCommand);
            activeCommand.Execute();
        }
    }

    public void Clear()
    {
        s_UndoStack.Clear();
        s_RedoStack.Clear();
    }
}
