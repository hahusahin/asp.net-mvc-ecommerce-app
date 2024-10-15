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
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Admin/Product/Index
        public ActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(products);
        }

        // GET: /Admin/Product/Upsert
        // GET: /Admin/Product/Upsert/:id
        public ActionResult Upsert(int? id)
        {
            //ViewBag.CategoryList = categoryList;
            //ViewData["CategoryList"] = categoryList;
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    }),
                Product = new Product()
            };
            if (id is null || id == 0)  // create
            {
                return View(productVM);
            } else  // update
            {
                productVM.Product = _unitOfWork.Product.Get(p => p.Id == id);
                return View(productVM);
            }
        }

        // POST: /Admin/Product/Upsert
        // POST: /Admin/Product/Upsert/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;  // Gives us the path of the wwwroot folder
                if (file is not null) 
                { 
                    string fileName =Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    // delete the old file as we are uploading new
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl)) 
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl);
                        if (System.IO.File.Exists(oldImagePath)) 
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // write the new file to the local ~/images/product folder
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"images\product\" + fileName;
                }

                // Create or Edit depending on the mode (has Id or not)
                if(productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                } else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }                

                _unitOfWork.Save();
                TempData["success"] = $"Product {(productVM.Product.Id == 0 ? "created" : "updated")} successfully";
                return RedirectToAction("Index");
            } else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    });
                return View(productVM);
            }

        }

        #region API CALLS
        [HttpGet]
        public ActionResult GetAll()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = products});
        }

        //DELETE  /Admin/Product/Delete/:id
        [HttpDelete]
        public ActionResult Delete(int? id)
        {
            Product? product = _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null) return Json(new {success = false, message = "Error while deleting" });

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl);
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}