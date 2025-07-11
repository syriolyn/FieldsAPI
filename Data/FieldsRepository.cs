using FieldsAPI.Models;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FieldsAPI.Data
{
    public class FieldsRepository : IFieldsRepository
    {
        private readonly List<Field> _fields;
        private readonly Dictionary<string, GeoPoint> _centroids;

        public FieldsRepository()
        {
            string basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data");
            string fieldsPath = Path.Combine(basePath, "fields.kml");
            string centroidsPath = Path.Combine(basePath, "centroids.kml");
            
            _fields = LoadFieldsFromKml(fieldsPath);
            _centroids = LoadCentroidsFromKml(centroidsPath);
            LinkFieldsWithCentroids();
        }

        private List<Field> LoadFieldsFromKml(string filePath)
        {
            var fields = new List<Field>();
            
            using (var stream = File.OpenRead(filePath))
            {
                var file = KmlFile.Load(stream);
                var kml = file.Root as Kml;
                var document = kml.Feature as Document;

                foreach (var placemark in document.Features.OfType<Placemark>())
                {
                    var polygon = placemark.Geometry as Polygon;
                    if (polygon == null) continue;

                    var coordinates = (polygon.OuterBoundary.LinearRing.Coordinates)
                        .Select(c => new GeoPoint { Lat = c.Latitude, Lng = c.Longitude })
                        .ToList();

                    fields.Add(new Field
                    {
                        Id = int.Parse(placemark.Id),
                        Name = placemark.Name,
                        Locations = new FieldGeometry
                        {
                            Polygon = coordinates
                        }
                    });
                }
            }

            return fields;
        }

        private Dictionary<string, GeoPoint> LoadCentroidsFromKml(string filePath)
        {
            var centroids = new Dictionary<string, GeoPoint>();
            
            using (var stream = File.OpenRead(filePath))
            {
                var file = KmlFile.Load(stream);
                var kml = file.Root as Kml;
                var document = kml.Feature as Document;

                foreach (var placemark in document.Features.OfType<Placemark>())
                {
                    var point = placemark.Geometry as SharpKml.Dom.Point;
                    if (point == null) continue;

                    centroids.Add(placemark.Id, new GeoPoint
                    {
                        Lat = point.Coordinate.Latitude,
                        Lng = point.Coordinate.Longitude
                    });
                }
            }

            return centroids;
        }

        private void LinkFieldsWithCentroids()
        {
            foreach (var field in _fields)
            {
                if (_centroids.TryGetValue(field.Id.ToString(), out var center))
                {
                    field.Locations.Center = center;
                    field.Size = CalculatePolygonArea(field.Locations.Polygon);
                }
            }
        }

        private double CalculatePolygonArea(List<GeoPoint> polygon)
        {
            double area = 0;
            int n = polygon.Count;
            
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += polygon[i].Lng * polygon[j].Lat;
                area -= polygon[j].Lng * polygon[i].Lat;
            }
            
            return Math.Abs(area) / 2.0 * 12387.1;
        }

        public IEnumerable<Field> GetAllFields() => _fields;

        public Field GetFieldById(int id) => _fields.FirstOrDefault(f => f.Id == id);

        public double CalculateFieldArea(int fieldId) => GetFieldById(fieldId)?.Size ?? 0;

        public double CalculateDistanceToCenter(int fieldId, GeoPoint point)
        {
            var field = GetFieldById(fieldId);
            if (field == null) return 0;

            return CalculateDistance(
                field.Locations.Center.Lat, 
                field.Locations.Center.Lng, 
                point.Lat, 
                point.Lng);
        }

        public Field CheckPointInFields(GeoPoint point)
        {
            return _fields.FirstOrDefault(f => IsPointInPolygon(point, f.Locations.Polygon));
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3;
            var φ1 = lat1 * Math.PI / 180;
            var φ2 = lat2 * Math.PI / 180;
            var Δφ = (lat2 - lat1) * Math.PI / 180;
            var Δλ = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static bool IsPointInPolygon(GeoPoint point, List<GeoPoint> polygon)
        {
            bool inside = false;
            int count = polygon.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                if (((polygon[i].Lat > point.Lat) != (polygon[j].Lat > point.Lat)) &&
                    (point.Lng < (polygon[j].Lng - polygon[i].Lng) * (point.Lat - polygon[i].Lat) / 
                    (polygon[j].Lat - polygon[i].Lat) + polygon[i].Lng))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}