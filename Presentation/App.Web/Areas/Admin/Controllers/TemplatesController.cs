//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using App.Services.Templates;
//using App.Core.Domain.Common;

//namespace App.Web.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class TemplatesController : Controller
//    {
//        private readonly ITemplateService _templates;
//        public TemplatesController(ITemplateService templates){ _templates = templates; }

//        [HttpGet]
//        public async Task<IActionResult> Index(string type=null)
//        {
//            var res = await _templates.GetAllAsync(type);
//            if (!res.Success) { TempData["Error"] = res.Error; return View(System.Array.Empty<Template>()); }
//            ViewBag.Type = type;
//            return View(res.Data);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id is null) return View(new Template());
//            var res = await _templates.GetAsync(id.Value);
//            if (!res.Success || res.Data == null) return NotFound();
//            return View(res.Data);
//        }

//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(Template model)
//        {
//            if (!ModelState.IsValid) return View(model);
//            if (model.Id == 0)
//            {
//                var cr = await _templates.CreateAsync(model);
//                if (!cr.Success) { TempData["Error"]=cr.Error; return View(model); }
//            }
//            else
//            {
//                var ur = await _templates.UpdateAsync(model.Id, model);
//                if (!ur.Success) { TempData["Error"]=ur.Error; return View(model); }
//            }
//            TempData["Success"] = "Saved";
//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var dr = await _templates.DeleteAsync(id);
//            TempData[dr.Success? "Success":"Error"] = dr.Success? "Deleted" : dr.Error;
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
