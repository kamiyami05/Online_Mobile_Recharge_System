using sem3.Models.Repositories;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly SettingsRepository _repo = new SettingsRepository();

        public ActionResult Index()
        {
            ViewBag.Address = _repo.GetValue("Contact_Address");
            ViewBag.PhoneMain = _repo.GetValue("Contact_PhoneMain");
            ViewBag.PhoneSupport = _repo.GetValue("Contact_PhoneSupport");
            ViewBag.Email1 = _repo.GetValue("Contact_Email1");
            ViewBag.Email2 = _repo.GetValue("Contact_Email2");
            ViewBag.HoursWeekdays = _repo.GetValue("Contact_HoursWeekdays");
            ViewBag.HoursWeekend = _repo.GetValue("Contact_HoursWeekend");

            return View();
        }
    }
}