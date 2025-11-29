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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _repository.Dispose();

            base.Dispose(disposing);
        }
    }
}