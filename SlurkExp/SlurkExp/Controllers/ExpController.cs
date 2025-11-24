using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlurkExp.Data;
using SlurkExp.Models;
using SlurkExp.Models.ViewModels;
using SlurkExp.Services.Hub;
using SlurkExp.Services.Settings;
using SlurkExp.Services.SlurkSetup;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlurkExp.Controllers
{
    public class ExpController : ControllerBase
    {
        private readonly IHubService _hubService;
        private readonly ISlurkExpRepository _repo;
        private readonly SlurkSetupOptions _setupOptions;
        private readonly SurveyOptions _surveyOptions;
        private readonly ISettingsService _settingsService;
        private readonly ISlurkSetup _slurkSetup;
        private readonly ILogger<ExpController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExpController(
            IHubService hubService,
            ISlurkExpRepository repo,
            IOptions<SlurkSetupOptions> setupOptions,
            IOptions<SurveyOptions> surveyOptions,
            ISettingsService settingsService,
            ISlurkSetup slurkSetup,
            ILogger<ExpController> logger)
        {
            _hubService = hubService;
            _repo = repo;
            _setupOptions = setupOptions.Value;
            _surveyOptions = surveyOptions.Value;
            _settingsService = settingsService;
            _slurkSetup = slurkSetup;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        [HttpGet("~/api/options")]
        public async Task<ActionResult> Options()
        {
            await Task.CompletedTask;
            return new JsonResult(_setupOptions);
        }

        [HttpGet("~/api/settings")]
        public async Task<ActionResult> Settings()
        {
            await Task.CompletedTask;
            return new JsonResult(_settingsService);
        }

        /// <summary>
        /// Start the experiment (without Prolific)
        /// </summary>
        [HttpGet("~/start/{id?}")]
        public async Task<ActionResult> Start([FromRoute] string id)
        {
            if (_settingsService.IsServerClosed)
            {
                var closeUrl = $"{_surveyOptions.Closed}";
                return Redirect("~/");
            }

            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("n");

            var client = await _repo.EnsureClient(id);

            await Log(client.ClientId, "Start", id);

            var qualtricsUrl = $"{_surveyOptions.Survey1}?token={client.ClientToken}";
            return Redirect(qualtricsUrl);
        }

        /// <summary>
        /// Start the experiment (without Prolific)
        /// </summary>
        [HttpGet("~/start-test/{id?}")]
        public async Task<ActionResult> StartTest([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("n");

            var client = await _repo.EnsureClient(id);

            await Log(client.ClientId, "StartTest", id);

            if (client != null)
            {
                await Log(client.ClientId, "Survey1Return", id);
                await _repo.SetClientStatus(id, 3);
                return Redirect($"{_setupOptions.BaseUrl}/login/?token={client.ChatToken}&name=Start-Test-{client.ClientToken.Substring(0, 6)}");
            }

            //var qualtricsUrl = $"{_surveyOptions.Survey1}?token={client.ClientToken}";
            //return Redirect(qualtricsUrl);

            return BadRequest();
        }

        /// <summary>
        /// Start directly in Slurk
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("~/slurk-direct/{id?}")]
        public async Task<ActionResult> SlurkDirect([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString("n");

            var client = await _repo.EnsureClient(id);

            if (client != null)
            {
                await Log(client.ClientId, "SlurkDirect", id);
                // TODO: specify status
                //await _repo.SetClientStatus(id, 3);
                return Redirect($"{_setupOptions.BaseUrl}/login/?token={client.ChatToken}&name=User{client.ChatName}");
            }

            await Log(1, "SlurkDirect-Fail", id);
            return Redirect("/");
        }

        /// <summary>
        /// A generic return route for testing only
        /// </summary>
        [HttpGet("~/return/{type}")]
        public async Task<ActionResult> Return([FromRoute] string type, [FromQuery] string token)
        {
            await Signal("Return", $"{type} {token}");

            string clientToken = "anonymous";
            var client = await _repo.GetClientByTokenId(token);
            if (client != null) clientToken = client.ClientToken;

            //_surveyOptions.Fail
            //https://app.prolific.com/submissions/complete?cc=C4C8C4XT

            switch (type)
            {
                case "wait":
                    await _repo.SetClientStatus(client.ClientToken, 9);
                    return Redirect($"{_surveyOptions.Fail}");
                //return Redirect($"{_surveyOptions.Fail}?type={type}&token={clientToken}");

                case "drop":
                    await _repo.SetClientStatus(client.ClientToken, 10);
                    return Redirect($"{_surveyOptions.Fail}");
                //return Redirect($"{_surveyOptions.Fail}?type={type}&token={clientToken}");

                case "chat": // SlurkSetupOptions__ChatRoomTimeoutUrl
                    return Redirect($"/slurk-return?type={type}&token={token}");

                case "chatx": // Test rerouting
                    return Redirect($"{_surveyOptions.Fail}?type={type}&token={clientToken}");

                default:
                    break;
            }

            return Redirect($"/result?type={type}&token={token}");
        }

        /// <summary>
        /// Entry point for Prolific participants
        /// </summary>
        [HttpGet("~/prolific")]
        public async Task<ActionResult> Prolific([FromQuery] string PROLIFIC_PID, [FromQuery] string SESSION_ID, [FromQuery] string STUDY_ID, [FromQuery] bool debug = false)
        {
            if (_settingsService.IsServerClosed)
            {
                var closeUrl = $"{_surveyOptions.Closed}";
                return Redirect(closeUrl);
            }

            if (string.IsNullOrEmpty(PROLIFIC_PID)) PROLIFIC_PID = Guid.NewGuid().ToString("n");

            var client = await _repo.EnsureClient(PROLIFIC_PID);

            await Log(client.ClientId, "Prolific", PROLIFIC_PID);

            var qualtricsUrl = $"{_surveyOptions.Survey1}?token={client.ClientToken}";

            if (debug) return new OkResult();

            return Redirect(qualtricsUrl);
        }

        /// <summary>
        /// API controller where qualtrics gets the treatment status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("~/survey1-checkin/{id?}")]
        public async Task<ActionResult> Survey1Checkin([FromRoute] string id)
        {
            var client = await _repo.GetClient(id);

            if (client != null)
            {
                await Log(client.ClientId, "Survey1Checkin", id);
                await _repo.SetClientStatus(id, 2);

                var treat = new
                {
                    info = client.Group.Treatment.Info,
                    positive = client.Group.Treatment.Positive,
                    topic = client.Group.Treatment.Topic,
                    treat = client.Group.TreatmentId,
                    group = client.Group.GroupId,
                    closed = 0
                };

                return new JsonResult(treat, _jsonOptions);
            }

            var treatFail = new
            {
                info = -1,
                positive = -1,
                topic = -1,
                treat = -1,
                group = -1,
                closed = 1
            };

            await Log(1, "Survey1Checkin-Fail", id);
            return new JsonResult(treatFail, _jsonOptions);
        }

        /// <summary>
        /// Returning from the first survey when close flag is activated.
        /// </summary>
        [HttpGet("~/survey1-closed/{id}")]
        public async Task<ActionResult> Survey1Closed([FromRoute] string id)
        {
            var client = await _repo.GetClient(id);
            if (client != null)
            {
                await Log(client.ClientId, "Survey1Closed", id);
                await _repo.SetClientStatus(id, 8);
                return Redirect(_surveyOptions.Closed);
            }

            // TODO: This will fail, no client id here
            await Log(1, "Survey1Closed-Fail", id);
            return Redirect(_surveyOptions.Closed);
        }

        /// <summary>
        /// API controller where qualtrics puts profile data
        /// </summary>
        [HttpPost("~/profile/{id?}")]
        public async Task<ActionResult> PutProfile([FromRoute] string id, [FromBody] ProfilePayload payload)
        {
            var client = await _repo.GetClient(id);
            if (client != null)
            {
                var json = JsonSerializer.Serialize(payload);
                await Log(client.ClientId, "PutProfile", $"{id} {json}");
                await _repo.SetClientProfile(id, payload);
                return Ok();
            }

            await Log(client.ClientId, "PutProfile-Fail", $"{id}");
            return BadRequest();
        }

        /// <summary>
        /// Returning from the first survey
        /// </summary>
        [HttpGet("~/survey1-return/{id}")]
        public async Task<ActionResult> Survey1Return([FromRoute] string id)
        {
            var client = await _repo.GetClient(id);
            if (client != null)
            {
                await Log(client.ClientId, "Survey1Return", id);
                await _repo.SetClientStatus(id, 3);
                return Redirect($"{_setupOptions.BaseUrl}/login/?token={client.ChatToken}&name={client.ChatName}");
            }

            await Log(1, "Survey1Return-Fail", id);
            return Redirect("/");
        }

        /// <summary>
        /// Returning from slurk chat
        /// </summary>
        [HttpGet("~/slurk-return")]
        public async Task<ActionResult> SlurkReturn([FromQuery] string token)
        {
            var client = await _repo.GetClientByTokenId(token);

            if (client != null)
            {
                await Log(client.ClientId, "SlurkReturn", token);
                await _repo.SetClientStatus(client.ClientToken, 5);
                var qualtricsUrl = $"{_surveyOptions.Survey2}?token={client.ClientToken}";
                return Redirect(qualtricsUrl);
            }

            await Log(1, "SlurkReturn", token);
            return Redirect($"/return/chatx?token={token}");
        }

        /// <summary>
        /// API controller where second qualtrics survey get the treatment status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("~/survey2-checkin/{id?}")]
        public async Task<ActionResult> Survey2Checkin([FromRoute] string id)
        {
            var client = await _repo.GetClient(id);

            if (client != null)
            {
                await Log(client.ClientId, "Survey2Checkin", id);
                await _repo.SetClientStatus(id, 6);

                var treat = new
                {
                    info = client.Group.Treatment.Info,
                    positive = client.Group.Treatment.Positive,
                    topic = client.Group.Treatment.Topic,
                    treat = client.Group.TreatmentId,
                    group = client.Group.GroupId,
                    bots = client.Group.Bots
                };

                return new JsonResult(treat, _jsonOptions);
            }

            var treatFail = new
            {
                info = -1,
                positive = -1,
                topic = -1,
                treat = -1,
                group = -1
            };

            await Log(1, "Survey2Checkin-Fail", id);
            return new JsonResult(treatFail, _jsonOptions);
        }

        /// <summary>
        /// Returning from the second survey
        /// </summary>
        [HttpGet("~/survey2-return/{id?}")]
        public async Task<ActionResult> Survey2Return([FromRoute] string id)
        {
            var client = await _repo.GetClient(id);

            if (client != null)
            {
                await Log(client.ClientId, "Survey2Return", id);
                await _repo.SetClientStatus(id, 7);
                return Redirect(_surveyOptions.End);
            }

            await Log(1, "Survey2Return-Fail", id);
            return Redirect("/");
        }


        /// <summary>
        /// Receive user left signal
        /// </summary>
        [HttpPost("~/api/user-notification")]
        public async Task<ActionResult> UserAction([FromBody] JsonElement json)
        {
            var userLeft = json.Deserialize<UserPayload>();
            _logger.LogInformation($"User {userLeft.UserId} left room {userLeft.RoomId}");
            await Signal("UserLeft", $"User {userLeft.UserId}  left room  {userLeft.RoomId}");
            return new OkResult();
        }

        /// <summary>
        /// Return the prompt for the bot
        /// </summary>
        [HttpGet("~/api/prompt")]
        public async Task<ActionResult> GetPrompt([FromQuery] int room_id = -1, [FromQuery] int bot_id = 0, [FromQuery] int client_id = 0)
        {
            await Log(1, "BotPrompt Enter", $"Bot {bot_id} in room {room_id}");

            if (_settingsService.TreatmentOverride > 0)
            {
                //room_id = _settingsService.RoomOverride;
            }

            if (room_id > 0)
            {
                var group = await _repo.Groups().Include(x => x.Treatment).FirstOrDefaultAsync(g => g.ChatRoomId == room_id);
                if (group != null)
                {
                    // Currently we override and spit out the default prompt
                    var prompt = await _repo.Prompts().FirstOrDefaultAsync(p => p.PromptId.Equals(group.Treatment.PromptId));
                    if (prompt != null)
                    {
                        // Basic Prompt
                        var content = prompt.Content;
                        string imitation = string.Empty;
                        string personalization = string.Empty;
                        Client client = new Client();

                        try
                        {
                            var clients = await _repo.Clients().OrderBy(x => x.ClientId).Where(x => x.Status >= 3 && x.Group.ChatRoomId.Equals(room_id)).ToListAsync();

                            if (clients.Count > 1)
                            {
                                var rand = new Random();
                                client = clients.OrderBy(x => x.ClientToken).First();
                                await _repo.SetClientComment(client.ClientToken, "ImitationSource");
                            }
                            else
                            {
                                client = clients.FirstOrDefault();
                            }

                            // Client override by url
                            if(client_id > 0)
                            {
                                client = await _repo.Clients().FirstOrDefaultAsync(x => x.ClientId.Equals(client_id));
                            }

                            // Client override, i.e., take json from specific client
                            if (_settingsService.ClientOverride > 0)
                            {
                                client = await _repo.Clients().FirstOrDefaultAsync(x => x.ClientId.Equals(_settingsService.ClientOverride));
                            }

                            var payload = JsonSerializer.Deserialize<ProfilePayload>(client.ClientJson);
                            payload.Topic = group.Treatment.Topic;

                            imitation = FormatImitation(payload);
                            personalization = FormatPersonalization(payload);
                        }
                        catch (Exception ex)
                        {
                            //TODO: Client with ID=1 does not always exists. Needs to seed db.
                            await Log(1, "PromptError", $"Exception for client {client.ClientId}: {ex.Message} {System.Environment.NewLine} {ex.InnerException}");
                        }

                        switch (group.Treatment.Category)
                        {
                            case 0:
                                // No specific treatment addition
                                break;
                            case 1:
                                // Control, no specific additions
                                break;
                            case 2:
                                // Control, no specific additions
                                break;
                            case 3:
                                // Baseline, no specific additions
                                break;
                            case 4:
                                // Repetion: Must change prompt fequency
                                break;
                            case 5:
                                // Imitation
                                content = content.Replace("#Customization#", imitation);
                                break;
                            case 6:
                                // Personalization
                                content = content.Replace("#Customization#", personalization);
                                break;
                            case 7:
                                // Combination
                                content = content.Replace("#Customization#", $"{imitation} {personalization}");
                                break;
                            default:
                                break;
                        }

                        await Log(1, "BotPrompt Exit", $"Bot {bot_id} in room {room_id} received PromptId {group.Treatment.PromptId}");

                        return Content(content);
                    }
                }
            }

            //if (bot_id > 0)
            //{
            //    var prompt = await _repo.Prompts().FirstOrDefaultAsync(p => p.BotId == bot_id);
            //    if (prompt != null)
            //    {
            //        return Content(prompt.Content);
            //    }
            //}

            return Content("Bot Prompt Error. Just tell people something is wrong.");
        }

        /// <summary>
        /// Return the prompt for the moderator
        /// </summary>
        [HttpGet("~/api/moderator-prompt")]
        public async Task<ActionResult> GetModeratorPrompt([FromQuery] int room_id = -1, [FromQuery] int bot_id = 0)
        {
            await Log(1, "ModeratorPrompt", $"Bot {bot_id} in room {room_id}");
            string prompt = "";

            // TODO: Remove room override
            //if (_settingsService.TreatmentOverride > 0)
            //{
            //    //room_id = _settingsService.RoomOverrideId;
            //}

            var group = await _repo.Groups().Include(x => x.Treatment).FirstOrDefaultAsync(g => g.ChatRoomId == room_id);
            if (group != null)
            {
                var minutes = Math.Round(_setupOptions.ChatRoomTimeoutSeconds / 60.0, 0);
                if (minutes == 0) minutes = 5;

                switch (group.Treatment.Topic)
                {
                    case 0:
                        prompt = $"Welcome! No chat topic is specified. Discuss anything.";
                        break;
                    case 1:
                        prompt = $"Welcome! This chat is open for {minutes} minutes. Discuss the following topic with the other participants: The government should fully subsidize public transport.";
                        break;
                    case 2:
                        prompt = $"Welcome! This chat is open for {minutes} minutes. Discuss the following topic with the other participants: All streets should have security camera surveillance.";
                        break;
                    case 3:
                        prompt = $"Welcome! This chat is open for {minutes} minutes. Discuss the following topic with the other participants: The organizers of a demonstration should pay the entire cost of the police protection needed.";
                        break;
                    case 4:
                        prompt = $"Welcome! This chat is open for {minutes} minutes. Discuss the following topic with the other participants: Foreigners who want a residence permit should fully pay for their integration courses and tests.";
                        break;
                    default:
                        break;
                }

                return Content(prompt);

                //var prompt = await _repo.Prompts().FirstOrDefaultAsync(p => p.BotId == (group.Treatment.Topic + 10));
                //if (prompt != null)
                //{
                //    return Content(prompt.Content);
                //}
            }

            // TODO: Not empty result, but error result, i.e., send away!
            await Log(1, "ModeratorPromptError", $"Bot {bot_id} in room {room_id}");
            return Content("Welcome! No chat topic is specified. Discuss anything.");
        }


        [HttpGet("~/api/profile-values")]
        public async Task<ActionResult> GetProfileValues([FromQuery] int client_id = 1, [FromQuery] int topic = 1)
        {
            var client = await _repo.Clients().FirstOrDefaultAsync(x => x.ClientId.Equals(client_id));
            if (client != null)
            {
                var payload = JsonSerializer.Deserialize<ProfilePayload>(client.ClientJson);
                payload.Topic = topic;
                var p = FormatProfile(payload);
                return new JsonResult(p);
            }

            return new JsonResult(new ProfileValue());
        }

        [HttpGet("~/api/get-clientlock")]
        public async Task<ActionResult> GetClientLock()
        {
            var locks = await _repo.GetClientLock();
            return new JsonResult(locks);
        }

        [HttpGet("~/api/set-clientlock")]
        public async Task<ActionResult> SetClientLock()
        {
            await _repo.SetClientLock();
            return Ok();
        }

        [HttpGet("~/api/get-grouplock")]
        public async Task<ActionResult> GetGroupLock()
        {
            var locks = await _repo.GetGroupLock();
            return new JsonResult(locks);
        }

        [HttpGet("~/api/set-grouplock")]
        public async Task<ActionResult> SetGroupLock()
        {
            await _repo.SetGroupLock();
            return Ok();
        }

        [HttpGet("~/api/release-locks")]
        public async Task<ActionResult> ReleaseLocks()
        {
            await _repo.ReleaseLocks();
            var client = await _repo.GetClientLock();
            var group = await _repo.GetGroupLock();
            var locks = new { client, group };
            return new JsonResult(locks);
        }


        /// <summary>
        /// Profile formatting for prompt injection
        /// </summary>
        private ProfileValue FormatProfile(ProfilePayload payload)
        {
            var topics = new string[] { "american politics", "public transport", "security cameras", "demonstrations", "integration" };
            var countries = new string[] { "United States", "United Kingdom" };
            var ages = new string[] { "18-24", "25-34", "35-44", "45-54", "55-64", "65-74", "75+" };
            var gender = new string[] { "woman", "man", "non-binary", "prefer to self-describe", "prefer not to say" };
            var education = new string[] { "no degree / education", "primary / elementary school", "middle school / high school", "professional / technical / vocational training", "college / university", "other" };
            var area = new string[] { "countryside", "small village or town", "suburb of city", "large city" };
            var interests = new string[] { "Sports", "Science", "Journalism", "Social Media", "Beauty", "Politics", "Gaming", "Food", "Business", "Nature", "Books", "Movies", "Travel", "Health", "Gardening", "Technology", "Space", "Music", "Arts", "Dancing", "History", "Philosophy", "Psychology", "Religion", "Theater", "Fashion", "Relationships" };

            var p = new ProfileValue();

            p.Country = countries[Convert.ToInt32(payload.Country) - 1];
            p.Gender = gender[Convert.ToInt32(payload.Gender) - 1];
            p.Age = ages[Convert.ToInt32(payload.Age) - 1];
            p.Education = education[Convert.ToInt32(payload.Education) - 1];
            p.Profession = payload.Profession;
            p.Area = area[Convert.ToInt32(payload.Area) - 1];
            p.Stance = payload.Stance;
            p.Name = payload.Name;

            var interestIndex = payload.Interests.Split(',')?.Select(Int32.Parse)?.ToList();
            List<string> interestList = new List<string>();
            foreach (var i in interestIndex) interestList.Add(interests[i - 1]);
            p.Interests = string.Join(", ", interestList);

            string[] transportArray = payload.Transport.Split(',');
            p.Transport = transportArray.Select(s => (int)Math.Round(double.Parse(s))).ToList();

            string[] demonstrationArray = payload.Demonstration.Split(',');
            p.Demonstration = demonstrationArray.Select(s => (int)Math.Round(double.Parse(s))).ToList();

            string[] integrationArray = payload.Integration.Split(',');
            p.Integration = integrationArray.Select(s => (int)Math.Round(double.Parse(s))).ToList();

            string[] cameraArray = payload.Camera.Split(',');
            p.Camera = cameraArray.Select(s => (int)Math.Round(double.Parse(s))).ToList();

            p.Topic = topics[payload.Topic];

            switch (p.Topic)
            {
                case "american politics":   // Should never be selcted !
                    p.Percentage1 = 0;
                    p.Percentage2 = 0;
                    p.Percentage3 = 0;
                    p.Percentage4 = 0;
                    p.Percentage5 = 0;
                    break;
                case "transport":
                    p.Percentage1 = p.Transport[0];
                    p.Percentage2 = p.Transport[1];
                    p.Percentage3 = p.Transport[2];
                    p.Percentage4 = p.Transport[3];
                    p.Percentage5 = p.Transport[4];
                    break;
                case "demonstrations":
                    p.Percentage1 = p.Demonstration[0];
                    p.Percentage2 = p.Demonstration[1];
                    p.Percentage3 = p.Demonstration[2];
                    p.Percentage4 = p.Demonstration[3];
                    p.Percentage5 = p.Demonstration[4];
                    break;
                case "integration":
                    p.Percentage1 = p.Integration[0];
                    p.Percentage2 = p.Integration[1];
                    p.Percentage3 = p.Integration[2];
                    p.Percentage4 = p.Integration[3];
                    p.Percentage5 = p.Integration[4];
                    break;
                case "security camera":
                    p.Percentage1 = p.Camera[0];
                    p.Percentage2 = p.Camera[1];
                    p.Percentage3 = p.Camera[2];
                    p.Percentage4 = p.Camera[3];
                    p.Percentage5 = p.Camera[4];
                    break;
                default:
                    p.Percentage1 = 0;
                    p.Percentage2 = 0;
                    p.Percentage3 = 0;
                    p.Percentage4 = 0;
                    p.Percentage5 = 0;
                    break;
            }

            return p;
        }

        /// <summary>
        /// Imitation formatting for prompt injection
        /// </summary>
        private string FormatImitation(ProfilePayload payload)
        {
            var p = FormatProfile(payload);

            // IMPLEMENTAION I
            //return $"Imitate {payload.Name}. This person is a {payload.Profession}.";

            //// IMPLEMENTAION II
            //string imitation = $"Imitate {p.Name}. This person is {p.Gender}, is {p.Age} old, is living in a {p.Area} in the {p.Country}. This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            //imitation += $"This person agrees with the topic {p.Percentage1} %." + Environment.NewLine;
            //imitation += $"This person is {p.Percentage2} % confident about their opinion on the topic." + Environment.NewLine;
            //imitation += $"This person thinks that the topic is {p.Percentage3} % important." + Environment.NewLine;
            //imitation += $"This person is {p.Percentage4} % emotional about the topic." + Environment.NewLine;
            //imitation += $"On a daily basis, this person is {p.Percentage5} % confronted with the topic.";
            //return imitation;

            //// IMPLEMENTAION III
            //string imitation = $"You are a {p.Gender}, are {p.Age} old, are living in a {p.Area} in the {p.Country}. Your education level is {p.Education}. You are a {p.Profession}. Your political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. You are interested in {p.Interests}." + Environment.NewLine;
            //return imitation;

            //// IMPLEMENTAION IV
            //string imitation = $"Imitate {p.Name}. This person is {p.Gender}, is {p.Age} old, is living in a {p.Area} in the {p.Country}. This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            //return imitation;

            // IMPLEMENTAION V
            string imitation = $"Imitate a person who is {p.Gender}, is {p.Age} old, is living in a {p.Area} in the {p.Country}. This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            return imitation;
        }

        /// <summary>
        /// Interests formatting for prompt injection
        /// </summary>
        private string FormatPersonalization(ProfilePayload payload)
        {
            var p = FormatProfile(payload);

            // IMPLEMENTAION I
            //var interestList = new string[] { "Sports", "Science", "Journalism", "Social Media", "Beauty", "Politics", "Gaming", "Food", "Business", "Nature", "Books", "Movies", "Travel", "Health", "Gardening", "Technology", "Space", "Music", "Arts", "Dancing" };
            //var interestIndex = payload.Interests.Split(',')?.Select(Int32.Parse)?.ToList();
            //List<string> interests = new List<string>();
            //foreach (var i in interestIndex) interests.Add(interestList[i - 1]);
            //return $"Your interests are one of the following: {string.Join(", ", interests)}. Try to mention this.";

            //// IMPLEMENTAION II
            //string personalization = $"Personalize the messages for {p.Name}. This person is {p.Gender}, is {p.Age} old, is living in a {p.Area} in {p.Country}. This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            //personalization += $"This person agrees with the topic {p.Percentage1} %." + Environment.NewLine;
            //personalization += $"This person is {p.Percentage2} % confident about their opinion on the topic." + Environment.NewLine;
            //personalization += $"This person thinks that the topic is {p.Percentage3} % important." + Environment.NewLine;
            //personalization += $"This person is {p.Percentage4} % emotional about the topic." + Environment.NewLine;
            //personalization += $"On a daily basis, this person is {p.Percentage5} % confronted with the topic.";
            //return personalization;

            //// IMPLEMENTAION III
            //string personalization = $"Personalize the messages for {p.Name}. This person is {p.Gender}, is {p.Age} old, is living in a {p.Area} in {p.Country}.This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            //personalization += $"This person agrees with the topic {p.Percentage1} %." + Environment.NewLine;
            //personalization += $"This person is {p.Percentage2} % confident about their opinion on the topic." + Environment.NewLine;
            //personalization += $"This person thinks that the topic is {p.Percentage3} % important." + Environment.NewLine;
            //personalization += $"This person is {p.Percentage4} % emotional about the topic." + Environment.NewLine;
            //personalization += $"On a daily basis, this person is {p.Percentage5} % confronted with the topic.";
            //return personalization;

            //// IMPLEMENTAION IV
            //string personalization = $"Personalize the messages for {p.Name}. This person is {p.Gender}, is {p.Age} old, is living in a {p.Area} in {p.Country}. This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            //return personalization;

            //// IMPLEMENTAION V
            string personalization = $"Personalize the messages for a person who is {p.Gender}, is {p.Age} old, is living in a {p.Area} in {p.Country}. This person's education level is {p.Education}. This person is a {p.Profession}. The person's political stance is {p.Stance}, on a scale where 0–4 means left-leaning, 5 is center, and 6–10 means right-leaning. This person is interested in {p.Interests}." + Environment.NewLine;
            return personalization;

        }

        /// <summary>
        /// Signal helper
        /// </summary>
        private async System.Threading.Tasks.Task Signal(string operation, string data)
        {
            await _hubService.SignalAgentHub($"{operation}: {data}");
        }

        /// <summary>
        /// Logging helper
        /// </summary>
        private async System.Threading.Tasks.Task Log(int clientId, string operation, string data)
        {
            await _hubService.SignalAgentHub($"{operation}: {clientId}: {data}");
            await _repo.AddLogEvent(clientId, operation, data);
        }
    }
}
