using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Models;
using SlurkExp.Services.ApiKey;
using SlurkExp.Services.Hub;
using SlurkExp.Services.SlurkSetup;
using System.Text.Json;

namespace SlurkExp.Controllers
{
    public class SlurkController : ControllerBase
    {
        private readonly SlurkExpDbContext _context;
        private readonly SlurkDbContext _slurkContext;
        private readonly ISlurkSetup _slurkSetup;
        private readonly IHubService _hubService;
        private readonly SurveyOptions _surveyOptions;
        private readonly ILogger<SlurkController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public SlurkController(
            SlurkExpDbContext context,
            SlurkDbContext slurkContext,
            ISlurkSetup slurkSetup,
            IHubService hubService,
            IOptions<SurveyOptions> surveyOptions,
            ILogger<SlurkController> logger)
        {
            _context = context;
            _slurkContext = slurkContext;
            _slurkSetup = slurkSetup;
            _hubService = hubService;
            _surveyOptions = surveyOptions.Value;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        [ApiKey]
        [HttpGet("~/api/slurk-db-test")]
        public async Task<ActionResult> SlurkDb()
        {
            var x = await _slurkContext.Logs.ToListAsync();
            return new JsonResult(x.Count, _jsonOptions);
        }

        [ApiKey]
        [HttpGet("~/api/exp-db-test")]
        public async Task<ActionResult> SlurkSetupDb()
        {
            var x = await _context.Clients.ToListAsync();
            return new JsonResult(x.Count, _jsonOptions);
        }

        [ApiKey]
        [HttpGet("~/api/survey-options")]
        public async Task<ActionResult> SurveyOptions()
        {
            await _hubService.SignalAgentHub($"Survey Options");
            return new JsonResult(_surveyOptions, _jsonOptions);
        }

        [ApiKey]
        [HttpGet("~/api/setup-options")]
        public async Task<ActionResult> SlurkSetupOptions()
        {
            await _hubService.SignalAgentHub($"Slurk Setup Options");
            var response = _slurkSetup.GetSetupOptions();
            return new JsonResult(response, _jsonOptions);
        }

        [ApiKey]
        [HttpGet("~/api/setup-request")]
        public async Task<ActionResult> SlurkSetupRequest()
        {
            await _hubService.SignalAgentHub($"Slurk Setup Request");
            var response = _slurkSetup.GetSetupRequest();
            return new JsonResult(response, _jsonOptions);
        }

        [ApiKey]
        [HttpGet("~/api/room-setup")]
        public async Task<ActionResult> SlurkSetup([FromQuery] int num_users = 1, [FromQuery] int min_users = 1, [FromQuery] string botId = "1", [FromQuery] string botName = "ChatBot")
        {
            await _hubService.SignalAgentHub($"Slurk Setup: {num_users}, {botId}");

            List<int> botIds = botId.Split(',').Select(int.Parse).ToList();
            List<string> botNames = botName.Split(',').Select(p => p.Trim()).ToList();
            
            var response = await _slurkSetup.RoomSetup(num_users, min_users, botIds, botNames);
            return new JsonResult(response, _jsonOptions);
        }

        [ApiKey]
        [HttpGet("~/api/treat-setup")]
        public async Task<ActionResult> TreatSetup([FromQuery] string id = "a")
        {
            if (!await _context.Treatments.AnyAsync())
            {
                var treat = new Treatment
                {
                    Name = "Treatment 1",
                    Info = 0,
                    Positive = 0,
                    Topic = 0,
                    Seats = 2,
                    Overbook = 0,
                    Bots = 1,
                    GroupCount = 5
                };

                _context.Treatments.Add(treat);
                await _context.SaveChangesAsync();
            }

            return Ok("Treatment seeded.");
        }

        [ApiKey]
        [HttpGet("~/api/group-setup")]
        public async Task<ActionResult> GroupSetup([FromQuery] string id = "a")
        {
            var treats = await _context.Treatments.ToListAsync();
            foreach (var t in treats)
            {
                var j = 1;
                for (int i = 0; i < t.GroupCount; i++)
                {
                    var g = new Group
                    {
                        TreatmentId = t.TreatmentId,
                        Name = $"Group {t.TreatmentId}-{j}",
                        Seats = t.Seats,
                        Overbook = t.Overbook,
                        Bots = t.Bots,
                        Checkin = 0,
                        Checkout = 0
                    };
                    _context.Groups.Add(g);
                    j++;
                }
            }
            await _context.SaveChangesAsync();

            return Ok("Groups seeded.");
        }

        [HttpGet("~/api/firstname")]
        public IActionResult FirstName()
        {
            var firstName = Faker.Name.First();
            return Content(firstName);
        }
    }
}
