using System.Collections.Generic;

public enum TurnPhase
{
    Placement,
    Action
}

public interface IGameModeRules
{
    VersusGameMode Mode { get; }
    int GetStartingPlayerIndex(List<IGamePlayer> players);
    int GetNextPlayerIndex(int currentPlayerIndex, List<IGamePlayer> players);
    /// <summary>
    /// Determines whether the acting player is allowed to move into the action phase for this game mode.
    /// </summary>
    bool CanEnterActionPhase(IGamePlayer player);
    bool AutoResolveActions(IGamePlayer player);
}

public class RoundRobinGameModeRules : IGameModeRules
{
    public virtual VersusGameMode Mode { get; protected set; }

    public RoundRobinGameModeRules(VersusGameMode mode)
    {
        Mode = mode;
    }

    public virtual int GetStartingPlayerIndex(List<IGamePlayer> players) => 0;

    public virtual int GetNextPlayerIndex(int currentPlayerIndex, List<IGamePlayer> players)
    {
        if (players == null || players.Count == 0)
        {
            return 0;
        }
        return (currentPlayerIndex + 1) % players.Count;
    }

    public virtual bool CanEnterActionPhase(IGamePlayer player) => player != null;

    public virtual bool AutoResolveActions(IGamePlayer player)
    {
        return player is AIPlayer;
    }
}

public class PvPGameModeRules : RoundRobinGameModeRules
{
    public PvPGameModeRules() : base(VersusGameMode.HumanVsHuman) { }

    public override bool AutoResolveActions(IGamePlayer player) => false;
}

public class PvEGameModeRules : RoundRobinGameModeRules
{
    public PvEGameModeRules() : base(VersusGameMode.HumanVsAI) { }
}

public class AISkirmishGameModeRules : RoundRobinGameModeRules
{
    public AISkirmishGameModeRules() : base(VersusGameMode.AIvsAI) { }
}

public static class GameModeRulesFactory
{
    public static IGameModeRules Create(VersusGameMode mode)
    {
        switch (mode)
        {
            case VersusGameMode.HumanVsHuman:
                return new PvPGameModeRules();
            case VersusGameMode.AIvsAI:
                return new AISkirmishGameModeRules();
            case VersusGameMode.HumanVsAI:
            case VersusGameMode.AIvsHuman:
            default:
                return new PvEGameModeRules();
        }
    }
}
