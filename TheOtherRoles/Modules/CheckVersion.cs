using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles.Modules;

internal class CheckVersion
{
    public static string FullVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    public class ApiResponse
    {
        public bool blackList { get; set; }
        public string message { get; set; }
        public int status { get; set; }
    }

    private static long GetBuiltInTicks()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var builtin = assembly.GetType("Builtin");
        if (builtin == null) return 0;
        var field = builtin.GetField("CompileTime");
        if (field == null) return 0;
        var value = field.GetValue(null);
        return value == null ? 0 : (long)value;
    }

    public static async Task checkBeta()
    {
        if (Main.betaDays > 0)
        {
            var ticks = GetBuiltInTicks();
            var compileTime = new DateTime(ticks, DateTimeKind.Utc);  // This may show as an error, but it is not, compilation will work!
            Message($"Beta版构建于: {compileTime.ToString(CultureInfo.InvariantCulture)}");
            DateTime? now;
            // Get time from the internet, so no-one can cheat it (so easily).
            try
            {
                var client = new System.Net.Http.HttpClient();
                using var response = await client.GetAsync("http://www.bing.com/");
                if (response.IsSuccessStatusCode)
                    now = response.Headers.Date?.UtcDateTime;
                else
                {
                    Message($"Could not get time from server: {response.StatusCode}");
                    now = DateTime.UtcNow; //In case something goes wrong. 
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                now = DateTime.UtcNow;
            }

            // Calculate the remaining days and store as an integer
            Main.BetaDaysLeft = (int)Math.Round(Main.betaDays - (now - compileTime)?.TotalDays ?? 0);

            if ((now - compileTime)?.TotalDays > Main.betaDays)
            {
                Message($"该Beta版本已过期! ");
                _ = BepInExUpdater.MessageBoxTimeout(BepInExUpdater.GetForegroundWindow(),
                    "该Beta版本已经过期, 请进行手动更新.\nBETA is expired. You cannot play this version anymore", "The Other Us - Edited", 0, 0, 10000);
                Application.Quit();
                return;
            }
            else
            {
                Message($"该Beta版本将在 {Main.BetaDaysLeft} 天后过期!");
            }
            Message("检查黑名单");
        }
    }
}
