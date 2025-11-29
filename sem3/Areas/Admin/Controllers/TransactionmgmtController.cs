using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System;
using System.Web.Mvc;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class TransactionmgmtController : Controller
    {
        private readonly TransactionRepository _repository = new TransactionRepository();

        public ActionResult Index()
        {
            var transactions = _repository.GetAll();
            return View(transactions);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _repository.Dispose();

            base.Dispose(disposing);
        }
    }
}