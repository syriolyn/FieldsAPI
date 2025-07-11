using System.Collections.Generic;

namespace FieldsAPI.Models
{
    public class GeoPoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class FieldGeometry
    {
        public GeoPoint Center { get; set; }
        public List<GeoPoint> Polygon { get; set; }
    }

    public class Field
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Size { get; set; }
        public FieldGeometry Locations { get; set; }
    }
}