using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using Telerik.Web.UI;

namespace ProjectPortal
{

    public partial class Calendar : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Page_Init(object sender, EventArgs e)
        {
            RadSchedulerCalendar.Provider = new ExchangeSchedulerProvider("https://webmail.alaska.gov/EWS/Exchange.asmx", "dnr.portaltest", "tazu+ebRe7r5zufuSebe", "soa.alaska.gov");
            //RadSchedulerCalendar.Provider = new ExchangeSchedulerProvider("https://webmail.alaska.gov/EWS/Exchange.asmx", "dpcard", "Caterpillar73", "soa.alaska.gov", "Test");

        }
    }
}