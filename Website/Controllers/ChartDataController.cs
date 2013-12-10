using System;
using System.Linq;
using System.Web.Http;
using process_runtime_monitor;
using Website.Models;

namespace Website.Controllers
{
    public class ChartDataController : ApiController
    {
        // GET api/<controller>
        public ChartModel Get()
        {
            var data = new ProcessStorage().GetProcessesFor("Boom", DateTime.Now.AddDays(-14).Date, DateTime.Now.Date);

            return new ChartModel
            {
                labels = data.Select(d => d.FriendlyRowKeyDate()).ToArray(), 
                datasets = new []{new Dataset
                {
                    fillColor = "rgba(151,187,205,0.5)",
                    strokeColor = "rgba(151,187,205,1)",
                    data = data.Select(d => d.TotalMinutesRun).ToArray()
                } }
            };
        }
    }
}