using CoffeeShopAPI.Data;
using CoffeeShopAPI.Data.DTOs;
using CoffeeShopAPI.Data.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //Add CORS policy name
    [EnableCors("AllowAll")]
    public class OrdersController : ControllerBase
    {
        private AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            var allOrders = _context.Orders.ToList();
            return Ok(allOrders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var orderById = _context.Orders.FirstOrDefault(n => n.Id == id);

            if(orderById == null)
            {
                return NotFound();
            } else
            {
                return Ok(orderById);
            }
        }

        [HttpGet("TotalOrdersSummary")]
        public IActionResult GetTotalOrdersSummary()
        {
            var totalOrders = _context.Orders.Count();

            return Ok($"Total orders = {totalOrders}");
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody]OrderDto orderDto)
        {
            var newOrder = new Order()
            {
                FullName = orderDto.FullName,
                Email = orderDto.Email,
                Description = orderDto.Description,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            return Ok(orderDto);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, [FromBody]OrderDto orderDto)
        {
            return Ok("UpdateOrder");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            return Ok($"Deleted order with id {id}");
        }
    }
}
