using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ContactSolution.Models;
using ContactSolution;
using System.IO;

namespace ContactSolution.Controllers
{
    public class ContactController : Controller
    {
        private readonly ITableManager repository;

        public ContactController(ITableManager repository)
        {
            this.repository = repository;
        }
        public IActionResult Index()
        {
            List<Contact> list = repository.GetAll().ToList();
            return View(list);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.Photo = Path.GetFileName(contact.PhotoFile.FileName);

                repository.UploadFileToBlob(contact.PhotoFile);

                repository.CreateOrUpdate(contact);

                repository.SendMessageToQueue(contact);

                return RedirectToAction("index", "contact");
            }

            return View();
        }
    }

}