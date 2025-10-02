using System.Globalization;
using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;

namespace Tests;

internal static class Utils
{
    internal static readonly FilterOperations[] s_numOps = [.. Enum.GetValues<FilterOperations>().Where(x => x < FilterOperations.Contains)];
    internal static readonly FilterOperations[] s_numOpsWoBetween = [.. Enum.GetValues<FilterOperations>().Where(x => x < FilterOperations.Between)];
    internal static readonly FilterOperations[] s_textOps =
    [
        .. Enum.GetValues<FilterOperations>().Where(x => x >= FilterOperations.Contains),
        .. new[] { FilterOperations.Equals, FilterOperations.NotEqual }
    ];

    internal static List<TheoryDataRow<string, FilterOperations>> GetValidIntCases()
    {
        List<TheoryDataRow<string, FilterOperations>> rows = [];

        var sep = FilterParsingOptions.Default.BetweenSeparator;

        string[] intValues = ["0", "123", "982211", "-1", "-1683242"];
        string[] intBetweenValues = [$"{sep}123", $"500{sep}", $"10{sep}1000", $"-852963{sep}0", $"0{sep}0"];

        var numOpsWoBetween = s_numOpsWoBetween[..^1];
        // Int
        foreach (var val in intValues)
        {
            foreach (FilterOperations op in numOpsWoBetween)
            {
                rows.Add((val, op));
            }
        }
        foreach (var val in intBetweenValues)
        {
            rows.Add((val, FilterOperations.Between));
        }
        return rows;
    }

    internal static List<TheoryDataRow<string, FilterOperations, string>> GetValidDecCases()
    {
        List<TheoryDataRow<string, FilterOperations, string>> rows = [];

        var sep = FilterParsingOptions.Default.BetweenSeparator;
        string[] cultures = ["en-US", "sr"];

        string[] decSrValues = ["0", "1,23", "21,19", "-1", "-991,018"];
        string[] decSrBetweenValues = [$"{sep}12,23", $"78,9{sep}", $"-1254,3{sep}919,3", $"{sep}-5,123", $"0{sep}8745,049"];

        string[] decEnValues = ["0", "1.23", "21.19", "-1", "-991.018"];
        string[] decEnBetweenValues = [$"{sep}12.23", $"78.9{sep}", $"-1254.3{sep}919.3", $"{sep}-5.123", $"0{sep}8745.049"];

        var numOpsWoBetween = s_numOpsWoBetween[..^1];
        // Decimal
        foreach (var culture in cultures)
        {
            var values = culture == "sr" ? decSrValues : decEnValues;
            foreach (var val in values)
            {
                foreach (FilterOperations op in numOpsWoBetween)
                {
                    rows.Add((val, op, culture));
                }
            }
            var betweenValues = culture == "sr" ? decSrBetweenValues : decEnBetweenValues;
            foreach (var val in betweenValues)
            {
                rows.Add((val, FilterOperations.Between, culture));
            }
        }
        return rows;
    }

    internal static List<TheoryDataRow<string, FilterOperations, string>> GetValidDateCases()
    {
        List<TheoryDataRow<string, FilterOperations, string>> rows = [];

        var sep = FilterParsingOptions.Default.BetweenSeparator;
        string[] cultures = ["en-US", "sr"];

        var now = DateOnly.FromDateTime(DateTime.Now);
        var srNow = now.ToString(new CultureInfo("sr"));
        var enNow = now.ToString(new CultureInfo("en-US"));
        var nm3ySr = now.AddYears(-3).ToString(new CultureInfo("sr"));
        var nm3yEn = now.AddYears(-3).ToString(new CultureInfo("en-US"));
        var np3ySr = now.AddYears(3).ToString(new CultureInfo("sr"));
        var np3yEn = now.AddYears(3).ToString(new CultureInfo("en-US"));

        string[] dtSrValues = ["22.01.2021", "12.07.2027", "13.09.2025", srNow];
        string[] dtSrBetweenValues = [$"{sep}12.07.2027", $"22.01.2021{sep}", $"13.09.2025{sep}12.07.2027", $"{nm3ySr}{sep}{np3ySr}"];

        string[] dtEnValues = ["01/22/2021", "07/12/2027", "09/13/2025", enNow];
        string[] dtEnBetweenValues = [$"{sep}07/12/2027", $"01/22/2021{sep}", $"09/13/2025{sep}07/12/2027", $"{nm3yEn}{sep}{np3yEn}"];

        var numOpsWoBetween = s_numOpsWoBetween[..^1];
        // Date
        foreach (var culture in cultures)
        {
            var values = culture == "sr" ? dtSrValues : dtEnValues;
            foreach (var val in values)
            {
                foreach (FilterOperations op in numOpsWoBetween)
                {
                    rows.Add((val, op, culture));
                }
            }
            var betweenValues = culture == "sr" ? dtSrBetweenValues : dtEnBetweenValues;
            foreach (var val in betweenValues)
            {
                rows.Add((val, FilterOperations.Between, culture));
            }
        }
        return rows;
    }

    internal static List<TheoryDataRow<string, FilterOperations>> GetValidStringCases()
    {
        List<TheoryDataRow<string, FilterOperations>> rows = [];
        string[] stringValues = ["test", "123", "", "test 123", "qwertyuiopasdfghjklzxcvbnm"];

        // Text
        foreach (var val in stringValues)
        {
            foreach (FilterOperations op in s_textOps)
            {
                rows.Add((val, op));
            }
        }

        return rows;
    }
}
