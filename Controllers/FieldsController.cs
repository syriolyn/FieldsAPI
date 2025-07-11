using FieldsAPI.Data;
using FieldsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FieldsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FieldsController : ControllerBase
    {
        private readonly IFieldsRepository _repository;

        public FieldsController(IFieldsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Field>> GetAllFields()
        {
            return Ok(_repository.GetAllFields());
        }

        [HttpGet("{id}/area")]
        public ActionResult<double> GetFieldArea(int id)
        {
            return Ok(_repository.CalculateFieldArea(id));
        }

        [HttpGet("{id}/distance")]
        public ActionResult<double> GetDistanceToCenter(
            int id, 
            [FromQuery] double lat, 
            [FromQuery] double lng)
        {
            return Ok(_repository.CalculateDistanceToCenter(id, new GeoPoint { Lat = lat, Lng = lng }));
        }

        [HttpGet("contains")]
        public ActionResult<Field> CheckPointInFields(
            [FromQuery] double lat, 
            [FromQuery] double lng)
        {
            var field = _repository.CheckPointInFields(new GeoPoint { Lat = lat, Lng = lng });
            return field != null ? Ok(field) : NotFound();
        }
    }
}