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
    private string endTime = "00:00:02";
    private string startDate = "2024-01-04";
    private string endDate = "2024-01-04";
    private HttpClient client = new HttpClient();


    [HttpGet("api/get-recording/")]
    public async Task<IActionResult> GetRecording()
    {
        // This method downloads a video from a camera.
        // It first retrieves the necessary information for the download request,
        // then sends the download request and checks if it was successful.

        // FIX: Downloaded videos are corrupted and cannot be played
        // TODO: Add error handling for the requests
        // TODO: Add a way to retrieve the video from the response
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
            string recordDownloadURL = $"http://{ip}/sdk.cgi?action=get.playback.download&chnid={chnid}&sid={sid}&streamType=primary&videoFormat=mp4&streamData=0&startTime={startDate}%20{startTime}&endTime={endDate}%20{endTime}";

            // get.playback.recordinfo request
            var recordDownloadResponse = await client.GetAsync(recordDownloadURL, HttpCompletionOption.ResponseHeadersRead);

            if (recordDownloadResponse.IsSuccessStatusCode)
            {
                using (var fileStream = System.IO.File.Create($"./Data/recordings/{sid}.mp4"))
                {
                    await recordDownloadResponse.Content.CopyToAsync(fileStream);
                }

                return Ok("Download successful");
            }
            else
            {
                return StatusCode((int)recordDownloadResponse.StatusCode, "Download failed");
            }
        }
        else
        {
            return StatusCode((int)recordInfoResponse.StatusCode, recordInfoResponse.ReasonPhrase + "Unable to retrieve record info");
        }

    }


}
