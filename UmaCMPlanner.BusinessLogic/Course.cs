using UmaCMPlanner.BusinessLogic.Enums;

namespace UmaCMPlanner.BusinessLogic;

public class Course
{
    public int Id { get; set; }
    public int Length { get; set; }
    public int PositionKeepEnd { get; set; }

    public List<CornerSection> Corners { get; set; } = new();
    public List<Section> Straights { get; set; } = new();
    public List<Section> NoMansLand { get; set; } = new();
    
    public List<Phase> Phases { get; set; } = new();
    public List<Slope> Slopes { get; set; } = new();

    public SpurtStart SpurtStart { get; set; }

    public List<Stat> StatThresholds { get; set; } = new();
}