using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Admin/Company/Index
        public ActionResult Index()
        {
            var companies = _unitOfWork.Company.GetAll();
            return View(companies);
        }

        // GET: /Admin/Company/Upsert
        // GET: /Admin/Company/Upsert/:id
        public ActionResult Upsert(int? id)
        {
            if (id is null || id == 0)  // create
            {
                return View(new Company());
            } else  // update
            {
                var company = _unitOfWork.Company.Get(p => p.Id == id);
                return View(company);
            }
        }

        // POST: /Admin/Company/Upsert
        // POST: /Admin/Company/Upsert/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)  // create
                {
                    _unitOfWork.Company.Add(company);
                } else
                {
                    _unitOfWork.Company.Update(company);
                }

                _unitOfWork.Save();
                TempData["success"] = $"Company {(company.Id == 0 ? "created" : "updated")} successfully";
                return RedirectToAction("Index");
            } else
            {
                return View(company);
            }
        }

        #region API CALLS
        [HttpGet]
        public ActionResult GetAll()
        {
            var companies = _unitOfWork.Company.GetAll().ToList();
            return Json(new {data = companies });
        }

        //DELETE  /Admin/Company/Delete/:id
        [HttpDelete]
        public ActionResult Delete(int? id)
        {
            Company? company = _unitOfWork.Company.Get(p => p.Id == id);
            if (company == null) return Json(new {success = false, message = "Error while deleting" });

            _unitOfWork.Company.Remove(company);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}