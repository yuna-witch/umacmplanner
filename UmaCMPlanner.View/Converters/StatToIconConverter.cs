using System.Globalization;
using System.Windows.Data;
using UmaCMPlanner.BusinessLogic.Enums;

namespace UmaCMPlanner.View.Converters;

public class StatToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Stat stat)
            return null!;

        return stat switch
        {
            Stat.Speed => "/Resources/StatIcons/Speed.png",
            Stat.Stamina => "/Resources/StatIcons/Stamina.png",
            Stat.Power => "/Resources/StatIcons/Power.png",
            Stat.Guts => "/Resources/StatIcons/Guts.png",
            Stat.Wit => "/Resources/StatIcons/Wit.png",
            _ => null!
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}