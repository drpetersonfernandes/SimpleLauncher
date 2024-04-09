using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleLauncher;

public static class Stats
{
    public static async Task CallApiAsync()
    {
        var client = new HttpClient();
        string apiUrl = "https://purelogiccode.com/simplelauncher/stats.php";
        string apiKey = "HJUYU675678rthgyjUIkder34343ghr43ehhhhhhJJJJJd";

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode(); // Exception if the HTTP response status is an error
       
        }
        catch (HttpRequestException ex)
        {
            await LogErrors.LogErrorAsync(ex, "There was an error communicating with the stats API");
        }
        catch (Exception e)
        {
            await LogErrors.LogErrorAsync(e, "There was an error communicating with the stats API");
        }
        finally
        {
            client.Dispose();
        }
    }
    
}