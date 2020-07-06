using System;
using System.Collections.Generic;
using ContactSolution.Models;
using Microsoft.AspNetCore.Http;

namespace ContactSolution
{
    public interface ITableManager
    {
        IEnumerable<Contact> GetAll();

        void CreateOrUpdate(Contact myTableOperation);

        void UploadFileToBlob(IFormFile file);
    }
}