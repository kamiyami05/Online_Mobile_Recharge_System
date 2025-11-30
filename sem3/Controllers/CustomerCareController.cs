using System;
using System.Linq;
using System.Web.Mvc;
using sem3.Models.Repositories;

namespace sem3.Controllers
{
    public class CustomerCareController : Controller
    {
        private readonly SettingsRepository _settingsRepo = new SettingsRepository();
        private readonly FAQRepository _faqRepo = new FAQRepository();

        public ActionResult Index()
        {
            ViewBag.Hotline = _settingsRepo.GetValue("Care_Hotline");
            ViewBag.Subtitle = _settingsRepo.GetValue("Care_Subtitle");
            ViewBag.SMSCode = _settingsRepo.GetValue("Care_SMS_Shortcode");
            return View();
        }
        public ActionResult GetFAQList(int page = 1)
        {
            int pageSize = 3;
            var allFaqs = _faqRepo.GetActiveFAQs();

            int totalItems = allFaqs.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedFaqs = allFaqs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return PartialView("_FAQList", pagedFaqs);
        }
    }
}