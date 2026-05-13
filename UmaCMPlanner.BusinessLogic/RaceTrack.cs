using UmaCMPlanner.BusinessLogic.Enums;

namespace UmaCMPlanner.BusinessLogic;

public class RaceTrack
{
    public Track Id { get; set; }
    
    public List<Course> Courses { get; set; }
}