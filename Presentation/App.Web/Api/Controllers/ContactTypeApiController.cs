using App.Core.Domain.Registrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ContactTypeApiController : ControllerBase
	{
		[HttpGet]
		public IActionResult GetAll()
		{
			var contactTypes = Enum.GetValues(typeof(ContactType))
				.Cast<ContactType>()
				.Select(e => new
				{
					Id = (int)e,
					Name = e.ToString()
				})
				.ToList();
			return Ok(contactTypes);
		}
	}
}
