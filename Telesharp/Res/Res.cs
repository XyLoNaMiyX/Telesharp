using System.Windows;
public static class Res
{
    static readonly FrameworkElement context = new FrameworkElement();
    
    public static T GetRes<T>(string name)
    { return (T)context.FindResource(name); }

    // returns null if the resource was not found
    public static string GetStr(string name, params object[] args)
    {
        try {
            var result = GetRes<string>(name);
            return args.Length > 0 ? string.Format(name, args) : name;
        }
        catch { return null; }
    }
}