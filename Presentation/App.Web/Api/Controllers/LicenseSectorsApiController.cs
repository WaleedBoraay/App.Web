using App.Core.Domain.Ref;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Web.Api.Controllers
{
    [Route("api/licensesectors")]
    [ApiController]
    public class LicenseSectorsApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            var sectors = Enum.GetValues(typeof(LicenseSector))
                .Cast<LicenseSector>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = e.ToString()
                })
                .ToList();

            return Ok(sectors);
        }
    }
}
