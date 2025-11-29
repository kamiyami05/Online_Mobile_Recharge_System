using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class PlanmgmtController : Controller
    {
        private readonly PlanRepository _repository = new PlanRepository();
        public ActionResult Index()
        {
            return View(_repository.GetAll());
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RechargePlan model)
        {
            ValidatePlanLogic(model);

            bool isDuplicate = _repository.GetAll().Any(p =>
                (p.PlanName.ToLower() == model.PlanName.ToLower()) ||
                (p.Operator == model.Operator && p.Amount == model.Amount && p.PlanType == model.PlanType)
            );

            if (isDuplicate)
            {
                ModelState.AddModelError("", "A plan with this Name OR same details already exists!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.IsActive == null) model.IsActive = true;
                    if (model.Details != null) model.Details = model.Details.Trim();

                    _repository.Create(model);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "System Error: " + ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var plan = _repository.GetById(id);
            if (plan == null) return HttpNotFound();
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RechargePlan model)
        {
            ValidatePlanLogic(model);

            bool isDuplicate = _repository.GetAll().Any(p =>
                p.PlanID != model.PlanID && (
                    (p.PlanName.ToLower() == model.PlanName.ToLower()) ||
                    (p.Operator == model.Operator && p.Amount == model.Amount && p.PlanType == model.PlanType)
                ));

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Another plan with same Name OR details already exists!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Details != null) model.Details = model.Details.Trim();

                    _repository.Update(model);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int id, bool status)
        {
            try
            {
                var plan = _repository.GetById(id);
                if (plan != null)
                {
                    plan.IsActive = status;
                    _repository.Update(plan);
                    return Json(new { success = true, message = "Status updated successfully!" });
                }
                return Json(new { success = false, message = "Plan not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var db = new OnlineRechargeDBEntities())
                {
                    bool isUsed = db.Transactions.Any(t => t.PlanID == id);
                    if (isUsed)
                    {
                        return Json(new { success = false, message = "Cannot delete! This plan has transaction history. Please Deactivate it instead." });
                    }
                }

                _repository.Delete(id);
                return Json(new { success = true, message = "Plan deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        private void ValidatePlanLogic(RechargePlan model)
        {
            if (model.PlanType == "Data" && (model.DataMB == null || model.DataMB <= 0))
            {
                ModelState.AddModelError("DataMB", "For Data Plans, Data (MB) must be greater than 0.");
            }
            if (model.PlanType == "Prepaid" && (model.TalkTimeMinutes == null || model.TalkTimeMinutes <= 0))
            {
                ModelState.AddModelError("TalkTimeMinutes", "For Prepaid Plans, Talk Time must be greater than 0.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _repository.Dispose();
            base.Dispose(disposing);
        }
    }
}