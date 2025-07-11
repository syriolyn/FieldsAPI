
using FieldsAPI.Models;
using System.Collections.Generic;

namespace FieldsAPI.Data
{
    public interface IFieldsRepository
    {
        IEnumerable<Field> GetAllFields();
        Field GetFieldById(int id);
        double CalculateFieldArea(int fieldId);
        double CalculateDistanceToCenter(int fieldId, GeoPoint point);
        Field CheckPointInFields(GeoPoint point);
    }
}