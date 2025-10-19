using App.Core.Domain.Ref;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Web.Api.Controllers
{
    [Route("api/financialdomains")]
    [ApiController]
    public class FinancialDomainsApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var domains = Enum.GetValues(typeof(FinancialDomain))
                .Cast<FinancialDomain>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                })
                .ToList();

            return Ok(domains);
        }
    }
}
