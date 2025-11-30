using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class UsermgmtController : Controller
    {
        private readonly UserRepository _repository = new UserRepository();
        public ActionResult Index()
        {
            var users = _repository.GetAll();
            return View(users);
        }
        [HttpPost]
        public ActionResult UpdateStatus(int id, bool status)
        {
            try
            {
                var user = _repository.GetById(id);

                if (user != null)
                {
                    user.Active = status;
                    _repository.Update(user);

                    return Json(new { success = true, message = "Status updated successfully!" });
                }

                return Json(new { success = false, message = "User not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _repository.Dispose();

            base.Dispose(disposing);
        }
    }
}