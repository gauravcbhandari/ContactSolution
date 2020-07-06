using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;

namespace ContactSolution.Models
{
    public class Contact : TableEntity
    {
        public Contact()
        {
            PartitionKey = "Accounts";
            RowKey = Guid.NewGuid().ToString();
        }

        [Required(ErrorMessage = "Please enter first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter cell number")]
        public string CellNo { get; set; }

        public string Photo { get; set; }

        [Required(ErrorMessage = "Please upload photo")]
        public IFormFile PhotoFile { get; set; }
    }
}