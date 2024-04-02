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

        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error
           
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


