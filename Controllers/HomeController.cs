using Microsoft.AspNetCore.Mvc;
using ProductTrackingSystem.Models;

namespace ProductTrackingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly List<StandardDowntime> _standardDowntimes = new List<StandardDowntime>
        {
            new StandardDowntime { Start = new TimeSpan(10, 0, 0), End = new TimeSpan(10, 15, 0), Reason = "Çay Molası" },
            new StandardDowntime { Start = new TimeSpan(12, 0, 0), End = new TimeSpan(12, 30, 0), Reason = "Yemek Molası" },
            new StandardDowntime { Start = new TimeSpan(15, 0, 0), End = new TimeSpan(15, 15, 0), Reason = "Çay Molası" }
        };

        [HttpGet]
        public IActionResult Index()
        {
            var model = new HomeViewModel();

            DateTime today = DateTime.Today;
            DateTime startDefault = today.AddHours(7).AddMinutes(30);
            DateTime endDefault = startDefault.AddHours(1);

            model.InputTable = new List<ProductionRecord>
            {
                new ProductionRecord
                {
                    RecordId = 1,
                    StartTime = startDefault,
                    EndTime = endDefault,
                    Status = ProductionStatus.URETIM
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Calculate(HomeViewModel model)
        {
            model.OutputTable = new List<ProductionRecord>();
            if (model.InputTable == null) model.InputTable = new List<ProductionRecord>();

            foreach (var inputRow in model.InputTable)
            {
                if (inputRow.RecordId == 0) continue;

                if (inputRow.Status == ProductionStatus.DURUS)
                {
                    model.OutputTable.Add(inputRow);
                    continue;
                }

                DateTime currentTracker = inputRow.StartTime;

                while (currentTracker < inputRow.EndTime)
                {
                    var currentDate = currentTracker.Date;

                    var nextBreak = _standardDowntimes
                        .Select(b => new
                        {
                            Start = currentDate + b.Start,
                            End = currentDate + b.End,
                            Reason = b.Reason
                        })
                        .Where(b => b.Start >= currentTracker && b.Start < inputRow.EndTime)
                        .OrderBy(b => b.Start)
                        .FirstOrDefault();

                    if (nextBreak != null)
                    {
                        if (nextBreak.Start > currentTracker)
                        {
                            model.OutputTable.Add(new ProductionRecord
                            {
                                RecordId = inputRow.RecordId,
                                StartTime = currentTracker,
                                EndTime = nextBreak.Start,
                                Status = ProductionStatus.URETIM
                            });
                        }

                        DateTime breakActualEnd = (nextBreak.End > inputRow.EndTime) ? inputRow.EndTime : nextBreak.End;

                        model.OutputTable.Add(new ProductionRecord
                        {
                            RecordId = inputRow.RecordId,
                            StartTime = nextBreak.Start,
                            EndTime = breakActualEnd,
                            Status = ProductionStatus.DURUS,
                            DowntimeReason = nextBreak.Reason
                        });

                        currentTracker = breakActualEnd;
                    }
                    else
                    {
                        model.OutputTable.Add(new ProductionRecord
                        {
                            RecordId = inputRow.RecordId,
                            StartTime = currentTracker,
                            EndTime = inputRow.EndTime,
                            Status = ProductionStatus.URETIM
                        });
                        currentTracker = inputRow.EndTime;
                    }
                }
            }

            var uniqueDates = model.InputTable
                .Where(x => x.RecordId != 0)
                .Select(x => x.StartTime.Date)
                .Distinct()
                .ToList();

            foreach (var date in uniqueDates)
            {
                foreach (var stdBreak in _standardDowntimes)
                {
                    DateTime breakStart = date + stdBreak.Start;
                    DateTime breakEnd = date + stdBreak.End;

                    bool isBreakExists = model.OutputTable.Any(x =>
                        x.StartTime == breakStart &&
                        x.DowntimeReason == stdBreak.Reason);

                    if (!isBreakExists)
                    {
                        model.OutputTable.Add(new ProductionRecord
                        {
                            RecordId = 0,
                            StartTime = breakStart,
                            EndTime = breakEnd,
                            Status = ProductionStatus.DURUS,
                            DowntimeReason = stdBreak.Reason
                        });
                    }
                }
            }

            model.OutputTable = model.OutputTable.OrderBy(x => x.StartTime).ToList();

            return View("Index", model);
        }
    }
}