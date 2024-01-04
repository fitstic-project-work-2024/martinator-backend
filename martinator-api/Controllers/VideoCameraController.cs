using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace martinator_api;

[ApiController]
public class VideoCameraController : ControllerBase
{
    private string ip = "93.57.67.110";
    private string user = "admin";
    private string password = "mutina23";
    private string startTime = "00:00:01";
    private string endTime = "00:00:05";
    private string startDate = "2024-01-04";
    private string endDate = "2024-01-04";
    HttpClient client = new HttpClient();

    [HttpGet("api/get-recording")]
    public async Task<IActionResult> GetRecording()
    {
        string authenticationString = $"{user}:{password}";
        var url = $"http://{ip}/sdk.cgi?action=get.playback.recordinfo&chnid=0&stream=0&startTime={startDate}%20{startTime}&endTime={endDate}%20{endTime}";

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Basic", Convert.ToBase64String(
           Encoding.ASCII.GetBytes(authenticationString)));

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
        else
        {
            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }

    }


}
