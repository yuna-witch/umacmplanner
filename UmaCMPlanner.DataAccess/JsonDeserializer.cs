using System.Text.Json.Nodes;
using UmaCMPlanner.BusinessLogic;
using UmaCMPlanner.BusinessLogic.Enums;

namespace UmaCMPlanner.DataAccess;

public static class JsonDeserializer
{
    public static List<ChampionsMeeting> ReadChampionsMeetingsFromJson()
    {
        string json = File.ReadAllText("champions-meetings.json");

        JsonArray data = JsonNode.Parse(json)!.AsArray();

        List<ChampionsMeeting> championsMeetings = data.Select(item => new ChampionsMeeting
        {
            Id = item["id"]!.GetValue<int>(),
            Name = item["name_en"]?.GetValue<string>()
                   ?? item["name"]?.GetValue<string>()
                   ?? "Unknown",

            Condition = (Condition)item["race"]!["condition"]!.GetValue<int>(),
            Length = item["race"]!["distance"]!.GetValue<int>(),
            Ground = (Ground)item["race"]!["ground"]!.GetValue<int>(),
            Season = (Season)item["race"]!["season"]!.GetValue<int>(),
            Track = (Track)item["race"]!["track"]!.GetValue<int>(),
            Turn = (Turn)item["race"]!["turn"]!.GetValue<int>(),
            Weather = (Weather)item["race"]!["weather"]!.GetValue<int>()
        }).ToList();

        foreach (var cm in championsMeetings)
        {
            cm.CourseId = GetRaceCourseId((int)cm.Track, (int)cm.Ground, cm.Length);
        }

        return championsMeetings;
    }

    private static int GetRaceCourseId(int trackId, int ground, int length)
    {
        string json = File.ReadAllText("racetracks.json");

        JsonArray data = JsonNode.Parse(json)!.AsArray();

        JsonNode? trackObject = data.FirstOrDefault(x =>
            x?["id"]?.GetValue<string>() == trackId.ToString());

        if (trackObject == null)
            return 0;

        JsonArray courses = trackObject["courses"]!.AsArray();

        var course = courses
            .FirstOrDefault(course =>
                course!["length"]!.GetValue<int>() == length &&
                course["terrain"]!.GetValue<int>() == ground);
        return course?["id"]?.GetValue<int>() ?? 0;
    }

    public static Dictionary<Track, RaceTrack> ReadRelevantTracksAndCourseFromJson(List<ChampionsMeeting> cmList)
    {
        var raceTracks = Enum.GetValues<Track>()
            .ToDictionary(
                t => t,
                t => new RaceTrack
                {
                    Id = t,
                    Courses = new List<Course>()
                });

        string json = File.ReadAllText("racetracks.json");

        JsonArray data = JsonNode.Parse(json)!.AsArray();

        foreach (var cm in cmList)
        {
            var courseNode = data.FirstOrDefault(x =>
                x?["id"]?.GetValue<string>() == ((int)cm.Track).ToString());

            if (courseNode == null)
                continue;

            var courses = courseNode["courses"]!.AsArray();

            var matchingCourse = courses.FirstOrDefault(c =>
                c!["id"]!.GetValue<int>() == cm.CourseId);

            if (matchingCourse == null)
                continue;

            int courseId = matchingCourse["id"]!.GetValue<int>();

            var track = raceTracks[cm.Track];

            if (track.Courses.All(c => c.Id != courseId))
            {
                raceTracks[cm.Track].Courses.Add(new Course
                {
                    Id = courseId,
                    Length = matchingCourse["length"]!.GetValue<int>(),
                    PositionKeepEnd = matchingCourse["positionKeepEnd"]!.GetValue<int>(),

                    Straights = matchingCourse["straights"]?
                        .AsArray()
                        .Select(s => new Section
                        {
                            Start = s!["start"]!.GetValue<int>(),
                            End = s["end"]!.GetValue<int>()
                        }).ToList() ?? new List<Section>(),
                    
                    NoMansLand = matchingCourse["noMansLand"]?
                        .AsArray()
                        .Select(n => new Section
                        {
                            Start = n!["start"]!.GetValue<int>(),
                            End = n["end"]!.GetValue<int>()
                        }).ToList() ?? new List<Section>(),

                    Corners = matchingCourse["corners"]?
                        .AsArray()
                        .Select(c => new CornerSection
                        {
                            Start = c!["start"]!.GetValue<int>(),
                            End = c["end"]!.GetValue<int>(),
                            Number = c["number"]!.GetValue<int>()
                        }).ToList() ?? new List<CornerSection>(),

                    Phases = matchingCourse["phases"]?
                        .AsArray()
                        .Select(p => new Phase
                        {
                            PhaseType = (PhaseType)p!["id"]!.GetValue<int>(),
                            Start = p["start"]!.GetValue<int>(),
                            End = p["end"]!.GetValue<int>()
                        }).ToList() ?? new(),

                    Slopes = matchingCourse["slopes"]?
                        .AsArray()
                        .Select(s => new Slope
                        {
                            Start = s!["start"]!.GetValue<int>(),
                            End = s["end"]!.GetValue<int>(),
                            Value = s["slope"]!.GetValue<int>()
                        }).ToList() ?? new(),
                    
                    StatThresholds = matchingCourse["statThresholds"]?
                        .AsArray()
                        .Select(x => (Stat)x!.GetValue<int>())
                        .ToList() ?? new(),
                    
                    SpurtStart = new SpurtStart
                        {
                            Meters = matchingCourse["spurtStart"]!["meters"]!.GetValue<int>(),
                            Location = matchingCourse["spurtStart"]!["location"]?
                                .AsArray()
                                .Select(x => x!.GetValue<string>())
                                .ToList() ?? new()
                        }
                });
            }
        }

        return raceTracks;
    }
}