using FluentValidationTestWebApp.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FluentValidationTestWebApp.Controllers
{
    public class ItemAsController : ODataController
    {
        [HttpGet]
        [ODataRoute("ItemAs")]
        public IEnumerable<ItemA> Get()
        {
            return new ItemA[]
            {
                new ItemA { Name = "A" },
            };
        }

        [HttpPost]
        [ODataRoute("ItemAs")]
        public IActionResult Post([FromBody] ItemA itemA)
        {
            return this.Created(itemA);
        }
    }
}
