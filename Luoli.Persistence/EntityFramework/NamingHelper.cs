using System.Text.RegularExpressions;

namespace Luoli.Persistence.EntityFramework;

/// <summary>
/// 命名转换辅助方法。
/// </summary>
public static class NamingHelper
{
    /// <summary>
    /// 将 PascalCase 转换为 snake_case。
    /// </summary>
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var leading = Regex.Match(input, @"^_+").Value;
        var body = input[leading.Length..];

        return leading + Regex.Replace(
            Regex.Replace(body, @"([a-z0-9])([A-Z])", "$1_$2"),
            @"([A-Z]+)([A-Z][a-z])", "$1_$2"
        ).ToLowerInvariant();
    }
}
