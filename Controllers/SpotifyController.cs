using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;
using Microsoft.Extensions.Configuration;
using Twilio.TwiML.Messaging;

namespace SpotifySMS.Controllers
{
    [Route("api/[controller]")]
    public class SpotifyController : Controller
    {
        protected IConfiguration Configuration {get;set;}
        public string SPOTIFY_ID {get; private set;}
        public string SPOTIFY_SECRET {get; private set;}
        public SpotifyController(IConfiguration config)
        {
            Configuration = config;
            SPOTIFY_ID = Configuration["CLIENT_ID"];
            SPOTIFY_SECRET = Configuration["CLIENT_SECRET"];
        }

        [HttpGet]
        public async Task<IActionResult> HttpGet([FromQuery] string body)
        {
            var spotify = new SpotifyClient(SPOTIFY_ID,SPOTIFY_SECRET);
            var song = await spotify.GetSong(body);

            var messagingResponse = new MessagingResponse();
            if (song == null)
            {
                messagingResponse.Message("Sending back a sample message since Spotify couldn't find our results!");
            }
            else
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Artist: {song["artist"].ToString()}");
                stringBuilder.AppendLine($"Title: {song["title"].ToString()}");
                var previewUrl = song["previewUrl"].ToString();
                if (string.IsNullOrEmpty(previewUrl))
                {
                    stringBuilder.AppendLine("Sorry we couldn't find a preview. But try visiting Spotify to listen to the full track");
                }
                else
                {
                    stringBuilder.AppendLine($"Here's the preview: {previewUrl}");

                }
                var message = new Message();
                message.Body(stringBuilder.ToString());
                message.Media(new Uri(song["image"].ToString()));
                messagingResponse.Append(message);
            }

            return new ContentResult{ Content = messagingResponse.ToString(), ContentType="application/xml"};
        }
    }
}