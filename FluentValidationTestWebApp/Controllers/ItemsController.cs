using FluentValidationTestWebApp.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FluentValidationTestWebApp.Controllers
{
    public class ItemsController : ODataController
    {
        [HttpGet]
        [ODataRoute("Items")]
        public IEnumerable<Item> Get()
        {
            return new Item[]
            {
                new ItemA { Name = "A" },
                new ItemB { Name = "B" },
            };
        }

        [HttpGet]
        [ODataRoute("Items/Item.ItemA")]
        public IActionResult GetFromItemA()
        {
            return this.Ok(new ItemA[]
            {
                new ItemA { Name = "A" },
            });
        }

        [HttpGet]
        [ODataRoute("Items/Item.ItemB")]
        public IActionResult GetFromItemB()
        {
            return this.Ok(new ItemB[]
            {
                new ItemB { Name = "B" },
            });
        }
    }
}
