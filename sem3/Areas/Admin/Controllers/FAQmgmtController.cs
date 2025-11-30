using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System;
using System.Web.Mvc;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class FAQmgmtController : Controller
    {
        private readonly FAQRepository _repo = new FAQRepository();

        public ActionResult Index()
        {
            return View(_repo.GetAll());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FAQ model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsActive == null) model.IsActive = true;
                _repo.Add(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = _repo.GetById(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FAQ model)
        {
            if (ModelState.IsValid)
            {
                _repo.Update(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int id, bool status)
        {
            try
            {
                var item = _repo.GetById(id);
                if (item != null)
                {
                    item.IsActive = status;
                    _repo.Update(item);
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                _repo.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _repo.Dispose();
            base.Dispose(disposing);
        }
    }
}