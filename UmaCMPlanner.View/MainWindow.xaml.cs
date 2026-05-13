using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UmaCMPlanner.UI;
using UmaCMPlanner.ViewModel;

namespace UmaCMPlanner.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CourseRenderer renderer;
    private MainWindowViewModel vm;
    
    public MainWindow()
    {
        InitializeComponent();

        vm = (MainWindowViewModel)DataContext;

        renderer = new CourseRenderer(TrackCanvas);

        vm.PropertyChanged += Vm_PropertyChanged;

        Loaded += MainWindow_Loaded;
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(vm.CourseOfSelectedCm))
        {
            renderer.Render(vm.CourseOfSelectedCm);
        }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            renderer.Render(vm.CourseOfSelectedCm);
        }
    }
}