using Microsoft.ML;
using Microsoft.ML.Trainers;
using OpinionRecommender.MLModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpinionRecommender.Services
{
    public class RecommendationService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<ProductRating, ProductRatingPrediction>? _predEngine;
        private readonly string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "recommendation_model.zip");
        private readonly string _datasetPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "ratings-data.csv");
        private List<ProductRating> _allRatings;

        public RecommendationService()
        {
            _mlContext = new MLContext(seed: 0); // Seed for reproducibility
            _allRatings = LoadRatingsFromCsv();
            LoadOrTrainModel();
        }

        private List<ProductRating> LoadRatingsFromCsv()
        {
            if (!File.Exists(_datasetPath))
            {
                // This case should ideally not happen if CSV is part of deployment
                // Or handle by creating a default/empty one or logging error
                return new List<ProductRating>();
            }

            var data = _mlContext.Data.LoadFromTextFile<ProductRating>(_datasetPath, hasHeader: true, separatorChar: ',');
            return _mlContext.Data.CreateEnumerable<ProductRating>(data, reuseRowObject: false).ToList();
        }

        private void LoadOrTrainModel()
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
            }
            else
            {
                if (_allRatings.Count == 0)
                {
                    // Cannot train if there's no data
                    // Potentially log an error or handle as appropriate
                    return;
                }
                var trainingDataView = _mlContext.Data.LoadFromEnumerable(_allRatings);

                var options = new MatrixFactorizationTrainer.Options
                {
                    LabelColumnName = "Label",
                    MatrixColumnIndexColumnName = _mlContext.Transforms.Conversion.MapValueToKey("UserId", "UserId").Fit(trainingDataView).Transform(trainingDataView).Schema["UserId"].Name,
                    MatrixRowIndexColumnName = _mlContext.Transforms.Conversion.MapValueToKey("ProductId", "ProductId").Fit(trainingDataView).Transform(trainingDataView).Schema["ProductId"].Name,
                    NumberOfIterations = 20,
                    ApproximationRank = 100,
                    Quiet = true
                };

                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "UserIdEncoded", inputColumnName: nameof(ProductRating.UserId))
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "ProductIdEncoded", inputColumnName: nameof(ProductRating.ProductId)))
                    .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                        labelColumnName: nameof(ProductRating.Label),
                        matrixColumnIndexColumnName: "UserIdEncoded",
                        matrixRowIndexColumnName: "ProductIdEncoded",
                        numberOfIterations: options.NumberOfIterations,
                        approximationRank: options.ApproximationRank
                        ));

                _model = pipeline.Fit(trainingDataView);
                _mlContext.Model.Save(_model, trainingDataView.Schema, _modelPath);
            }

            if (_model != null)
            {
                _predEngine = _mlContext.Model.CreatePredictionEngine<ProductRating, ProductRatingPrediction>(_model);
            }
        }

        public List<(string ProductId, float Score)> Recommend(string userId, int topN = 5)
        {
            if (_predEngine == null || _allRatings.Count == 0) return new List<(string, float)>();

            var userToUpper = userId.ToUpperInvariant();

            // Get products the user has already rated
            var productosCalificados = _allRatings
                .Where(r => r.UserId.Equals(userToUpper, System.StringComparison.OrdinalIgnoreCase))
                .Select(r => r.ProductId)
                .ToHashSet();

            // Get all unique product IDs from the static list (as this contains metadata like names/images)
            var allPossibleProductIds = DatosFicticios.Productos.Select(p => p.ProductId).ToList();

            var recommendations = new List<(string ProductId, float Score)>();

            foreach (var productId in allPossibleProductIds)
            {
                // Only recommend products the user hasn't rated yet
                if (!productosCalificados.Contains(productId))
                {
                    var prediction = _predEngine.Predict(new ProductRating { UserId = userToUpper, ProductId = productId });
                    if (!float.IsNaN(prediction.Score)) // Ensure score is a valid number
                    {
                        recommendations.Add((productId, prediction.Score));
                    }
                }
            }

            // If no new recommendations (e.g., user rated all or new user not in training data for some specific items),
            // or to ensure we always have some items, predict for all products.
            // This part might need refinement based on desired behavior for new users or fully rated users.
            if (recommendations.Count == 0 && allPossibleProductIds.Count > 0)
            {
                 foreach (var productId in allPossibleProductIds)
                 {
                    // We can opt to not re-predict for already rated items or show them with their actual rating
                    var prediction = _predEngine.Predict(new ProductRating { UserId = userToUpper, ProductId = productId });
                    if (!float.IsNaN(prediction.Score))
                    {
                        recommendations.Add((productId, prediction.Score));
                    }
                 }
            }

            return recommendations.OrderByDescending(x => x.Score).Take(topN).ToList();
        }
    }
}
