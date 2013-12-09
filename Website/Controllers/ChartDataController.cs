using System.Web.Http;
using Website.Models;

namespace Website.Controllers
{
    public class ChartDataController : ApiController
    {
        // GET api/<controller>
        public ChartModel Get()
        {
            return new ChartModel
            {
                labels = new[] { "January", "February", "March", "April", "May", "June", "July" }, 
                datasets = new []{new Dataset
                {
                    fillColor = "rgba(151,187,205,0.5)",
                    strokeColor = "rgba(151,187,205,1)",
                    data = new decimal[] {28, 48, 40, 19, 96, 27, 100}
                } }
            };
        }
    }
}