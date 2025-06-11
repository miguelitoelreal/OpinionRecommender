using Microsoft.ML.Data;
using System.Collections.Generic;

namespace OpinionRecommender.MLModel
{
    public class ProductRating
    {
        [LoadColumn(0)] public string UserId { get; set; } = string.Empty;
        [LoadColumn(1)] public string ProductId { get; set; } = string.Empty;
        [LoadColumn(2)] public float Label { get; set; }
    }

    public class ProductRatingPrediction
    {
        public float Score { get; set; }
    }

    // Datos ficticios para recomendación
    public static class DatosFicticios
    {
        public static List<string> Usuarios = new() { "U1", "U2", "U3", "U4", "U5" };
        public static List<(string ProductId, string Nombre, string Imagen)> Productos = new()
        {
            ("P1", "Videojuego Mario", "https://images.unsplash.com/photo-1519125323398-675f0ddb6308?auto=format&fit=crop&w=400&q=80"),
            ("P2", "Libro de Fantasía", "https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=400&q=80"),
            ("P3", "Muñeca Barbie", "https://images.unsplash.com/photo-1519985176271-adb1088fa94c?auto=format&fit=crop&w=400&q=80"),
            ("P4", "Película Animada", "https://images.unsplash.com/photo-1465101046530-73398c7f28ca?auto=format&fit=crop&w=400&q=80"),
            ("P5", "Serie de Aventuras", "https://images.unsplash.com/photo-1519121782439-2c5f2c2a3b89?auto=format&fit=crop&w=400&q=80")
        };
        public static List<ProductRating> Interacciones = new()
        {
            new ProductRating { UserId = "U1", ProductId = "P1", Label = 5 },
            new ProductRating { UserId = "U1", ProductId = "P2", Label = 4 },
            new ProductRating { UserId = "U1", ProductId = "P3", Label = 5 },
            new ProductRating { UserId = "U2", ProductId = "P2", Label = 1 },
            new ProductRating { UserId = "U2", ProductId = "P4", Label = 2 },
            new ProductRating { UserId = "U3", ProductId = "P1", Label = 4 },
            new ProductRating { UserId = "U3", ProductId = "P4", Label = 5 },
            new ProductRating { UserId = "U3", ProductId = "P5", Label = 3 },
            new ProductRating { UserId = "U4", ProductId = "P3", Label = 2 },
            new ProductRating { UserId = "U4", ProductId = "P5", Label = 4 },
            new ProductRating { UserId = "U5", ProductId = "P2", Label = 5 },
            new ProductRating { UserId = "U5", ProductId = "P3", Label = 4 },
            new ProductRating { UserId = "U5", ProductId = "P4", Label = 4 }
        };
    }
}
