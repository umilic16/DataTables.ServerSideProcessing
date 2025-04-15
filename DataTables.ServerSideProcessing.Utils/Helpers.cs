namespace DataTables.ServerSideProcessing.Utils;
internal static class Helpers
{
    internal static int ToInt(this string broj)
    {
        return int.TryParse(broj, out int result) ? result : -1;
    }
}
