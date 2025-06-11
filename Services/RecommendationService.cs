using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Recommender;
using Microsoft.ML.Trainers;
using OpinionRecommender.MLModel;
using System.Linq;
using System.IO;

namespace OpinionRecommender.Services
{
    public class RecommendationService
    {
        private readonly string _modelPath = "MLModel/ratings-data.csv";
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<ProductRating, ProductRatingPrediction>? _predEngine;
        private IDataView? _dataView;

        public RecommendationService()
        {
            _mlContext = new MLContext();
            LoadDataAndTrain();
        }

        private void LoadDataAndTrain()
        {
            // Entrenar modelo desde datos ficticios en memoria (sin archivos temporales)
            _dataView = _mlContext.Data.LoadFromEnumerable(DatosFicticios.Interacciones);
            var dataProcessPipeline = _mlContext.Transforms.Conversion.MapValueToKey("UserId")
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("ProductId"));
            var options = new MatrixFactorizationTrainer.Options
            {
                LabelColumnName = "Label",
                MatrixColumnIndexColumnName = "UserId",
                MatrixRowIndexColumnName = "ProductId"
            };
            var pipeline = dataProcessPipeline.Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));
            _model = pipeline.Fit(_dataView);
            _predEngine = _mlContext.Model.CreatePredictionEngine<ProductRating, ProductRatingPrediction>(_model);
        }

        public List<(string ProductId, float Score)> Recommend(string userId, int topN = 5)
        {
            if (_dataView == null || _predEngine == null) return new List<(string, float)>();
            // Recomendar solo productos que el usuario NO ha calificado
            var productosCalificados = DatosFicticios.Interacciones.Where(x => x.UserId == userId).Select(x => x.ProductId).ToHashSet();
            var products = DatosFicticios.Productos.Select(x => x.ProductId).Where(pid => !productosCalificados.Contains(pid)).ToList();
            var recommendations = new List<(string, float)>();
            foreach (var productId in products)
            {
                var prediction = _predEngine.Predict(new ProductRating { UserId = userId, ProductId = productId });
                recommendations.Add((productId, prediction.Score));
            }
            // Si el usuario ya calificó todos, igual mostrar los topN de todos
            if (recommendations.Count == 0)
            {
                products = DatosFicticios.Productos.Select(x => x.ProductId).ToList();
                foreach (var productId in products)
                {
                    var prediction = _predEngine.Predict(new ProductRating { UserId = userId, ProductId = productId });
                    recommendations.Add((productId, prediction.Score));
                }
            }
            return recommendations.OrderByDescending(x => x.Item2).Take(topN).ToList();
        }
    }
}
