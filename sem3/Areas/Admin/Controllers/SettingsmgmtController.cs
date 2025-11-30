using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System.Collections.Generic;
using System.Web.Mvc;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class SettingsmgmtController : Controller
    {
        private readonly SettingsRepository _repository = new SettingsRepository();

        public ActionResult Index()
        {
            return View(_repository.GetAll());
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateAll(List<SystemSetting> settings)
        {
            if (settings != null)
            {
                foreach (var item in settings)
                {
                    var dbItem = _repository.GetByKey(item.SettingKey);
                    if (dbItem != null)
                    {
                        dbItem.SettingValue = item.SettingValue;
                        _repository.Update(dbItem);
                    }
                }
            }
            TempData["Message"] = "All settings updated successfully!";
            return RedirectToAction("Index");
        }
    }
}