using Microsoft.ML;
using OpinionRecommender.MLModel;
using System.IO;

namespace OpinionRecommender.Services
{
    public class SentimentService
    {
        private MLContext? _mlContext;
        private ITransformer? _model;
        private PredictionEngine<SentimentData, SentimentPrediction>? _predEngine;
        private string? _error;
        public string? Error => _error;

        public SentimentService()
        {
            try
            {
                _mlContext = new MLContext();
                var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "sentiment-data.tsv");
                if (!File.Exists(dataPath))
                {
                    _error = $"No se encontró el dataset en: {dataPath}";
                    return;
                }
                var dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(dataPath, hasHeader: true);
                var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                    .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());
                _model = pipeline.Fit(dataView);
                _predEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            }
            catch (Exception ex)
            {
                _error = ex.Message;
            }
        }

        public SentimentPrediction? Predict(string text)
        {
            if (_error != null || _predEngine == null) return null;
            var input = new SentimentData { Text = text };
            return _predEngine.Predict(input);
        }
    }
}
