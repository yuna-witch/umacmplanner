using UmaCMPlanner.BusinessLogic.Enums;

namespace UmaCMPlanner.ViewModel.Helpers;

public class EnumFormatter
{
    public static string FormatTurn(Turn turn)
    {
        return turn switch
        {
            Turn.LeftHanded => "Left-Handed",
            Turn.RightHanded => "Right-Handed",
            _ => turn.ToString()
        };
    }
    
    public static string FormatCondition(Condition condition)
    {
        return condition switch
        {
            Condition.Firm => "Firm",
            _ => "Wet"
        };
    }
}