using System.ComponentModel;
using System.Runtime.CompilerServices;
using UmaCMPlanner.BusinessLogic;
using UmaCMPlanner.BusinessLogic.Enums;
using UmaCMPlanner.DataAccess;

namespace UmaCMPlanner.ViewModel;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        ChampionsMeetings = JsonDeserializer.ReadChampionsMeetingsFromJson();
        SelectedCm = ChampionsMeetings[0];
        RaceTracks = JsonDeserializer.ReadRelevantTracksAndCourseFromJson(ChampionsMeetings);

        LeftButtonCommand = new RelayCommand(LeftButtonClicked);
        RightButtonCommand = new RelayCommand(RightButtonClicked);
    }

    public string Title => $"Champions Meeting {SelectedCm.Id}";

    public ChampionsMeeting SelectedCm
    {
        get;
        private set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(CourseOfSelectedCm));
        }
    }
    
    public Course CourseOfSelectedCm => GetCourseForSelectedCm();

    public RelayCommand LeftButtonCommand { get; }

    public RelayCommand RightButtonCommand { get; }
    
    private List<ChampionsMeeting> ChampionsMeetings { get; set; }
    
    private Dictionary<Track, RaceTrack> RaceTracks { get; set; }

    private Course GetCourseForSelectedCm()
    {
        return RaceTracks[SelectedCm.Track].Courses.FirstOrDefault(c => c.Id == SelectedCm.CourseId)!;
    }
    
    private void LeftButtonClicked()
    {
        SelectedCm = SelectedCm == ChampionsMeetings[0] ? ChampionsMeetings[^1] : ChampionsMeetings[ChampionsMeetings.IndexOf(SelectedCm) - 1];
    }

    private void RightButtonClicked()
    {
        SelectedCm = SelectedCm == ChampionsMeetings[^1] ? ChampionsMeetings[0] : ChampionsMeetings[ChampionsMeetings.IndexOf(SelectedCm) + 1];
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}