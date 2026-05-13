using UmaCMPlanner.BusinessLogic.Enums;

namespace UmaCMPlanner.BusinessLogic;

public class ChampionsMeeting
{
    private int length;
    
    public int Id { get; set; }
    public string Name { get; set; }

    public Condition Condition { get; set; }
    public int Length
    {
        get => length;
        set
        {
            length = value;
            switch (length)
            {
                case < 1600:
                    Distance = Distance.Sprint;
                    break;
                case < 2000:
                    Distance = Distance.Mile;
                    break;
                case < 2500:
                    Distance = Distance.Medium;
                    break;
                default:
                    Distance = Distance.Long;
                    break;
            } 
        }
    }
    
    public Distance Distance { get; set; }
    public Ground Ground { get; set; }
    public Season Season { get; set; }
    public Track Track { get; set; }
    public int CourseId { get; set; }
    public Turn Turn { get; set; }
    public Weather Weather { get; set; }
}