using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;


namespace martinator_api;

[ApiController]
public class VideoCameraController : ControllerBase
{
    // Data needed to access the camera recording, they are hardcoded for testing purposes
    private string user = "admin";
    private string password = "mutina23";
    private string ip = "93.57.67.110";
    private string startTime = "00:00:01";
    private string endTime = "00:00:60";
    private string startDate = "2024-01-04";
    private string endDate = "2024-01-04";
    private HttpClient client = new HttpClient();


    [HttpGet("api/get-recording/")]
    public async Task<IActionResult> GetRecording()
    {
        // This method downloads a video from a camera.
        // It first retrieves the necessary information for the download request,
        // then sends the download request and checks if it was successful.

        // TODO: Add error handling for the request and the curl command
        // TODO: Set needed parameters as a query in the URL


        string authenticationString = $"{user}:{password}";
        string recordInfoURL = $"http://{ip}/sdk.cgi?action=get.playback.recordinfo&chnid=0&stream=0&startTime={startDate}%20{startTime}&endTime={endDate}%20{endTime}";

        // Setting Authentication header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Basic", Convert.ToBase64String(
           Encoding.ASCII.GetBytes(authenticationString)));

        // get.playback.recordinfo request
        var recordInfoResponse = await client.GetAsync(recordInfoURL);

        if (recordInfoResponse.IsSuccessStatusCode)
        {
            var content = await recordInfoResponse.Content.ReadAsStringAsync();

            // Parsing recordInfoResponse 
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var recordInfoValues = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    recordInfoValues[parts[0]] = parts[1];
                }
            }

            // Retrieving needed values for the get.playback.download request
            string chnid = recordInfoValues["chnid"];
            string sid = recordInfoValues["sid"];
            string size = recordInfoValues["allSize"];
            string recordDownloadURL = $"http://{ip}/sdk.cgi?action=get.playback.download&chnid={chnid}&sid={sid}&streamType=primary&videoFormat=mp4&streamData=1&startTime={startDate}%20{startTime}&endTime={endDate}%20{endTime}";

            // get.playback.recordinfo request through curl process
            var startInfo = new ProcessStartInfo
            {
                FileName = "curl",
                Arguments = $"-o ./Data/recordings/NVR-CAM-S{formatDate(startDate)}-{formatTime(startTime)}-E{formatDate(endDate)}-{formatTime(endTime)}.mp4 -u admin:mutina23  {recordDownloadURL}",
                UseShellExecute = false,
            };
            var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync();

            return Ok("Video successfully downloaded");


        }
        else
        {
            return StatusCode((int)recordInfoResponse.StatusCode, recordInfoResponse.ReasonPhrase + "Unable to retrieve record info");
        }


    }

    public string formatDate(string date)
    {
        // This method formats a date from yyyy-mm-dd to yyyymmdd
        string[] dateParts = date.Split("-");
        string formattedDate = dateParts[0] + dateParts[1] + dateParts[2];
        return formattedDate;
    }

    public string formatTime(string time)
    {
        // This method formats a time from hh:mm:ss to hhmmss
        string[] timeParts = time.Split(":");
        string formattedTime = timeParts[0] + timeParts[1] + timeParts[2];
        return formattedTime;
    }
}
