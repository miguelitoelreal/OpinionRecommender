using Microsoft.AspNetCore.Mvc;
using OpinionRecommender.Services;
using OpinionRecommender.MLModel;

namespace OpinionRecommender.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _service;
        public RecommendationController(RecommendationService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string userId, [FromQuery] int topN = 5)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Debe ingresar un UserId");
            var userIdUpper = userId.ToUpperInvariant();
            var result = _service.Recommend(userIdUpper, topN)
                .Select(x => new {
                    ProductId = x.ProductId,
                    Score = x.Score,
                    Producto = DatosFicticios.Productos.FirstOrDefault(p => p.ProductId == x.ProductId)
                })
                .ToList();
            return Ok(result);
        }
    }
}
