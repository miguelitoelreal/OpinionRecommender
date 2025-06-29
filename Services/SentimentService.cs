using Microsoft.ML;
using OpinionRecommender.MLModel;
using System;
using System.IO;

namespace OpinionRecommender.Services
{
    public class SentimentService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<SentimentData, SentimentPrediction>? _predEngine;
        private string? _error;
        public string? Error => _error;

        private readonly string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "sentiment_model.zip");
        private readonly string _datasetPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "sentiment-data.tsv");

        public SentimentService()
        {
            _mlContext = new MLContext(seed: 0); // Seed for reproducibility
            LoadOrTrainModel();
        }

        private void LoadOrTrainModel()
        {
            try
            {
                if (File.Exists(_modelPath))
                {
                    _model = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
                }
                else
                {
                    if (!File.Exists(_datasetPath))
                    {
                        _error = $"No se encontró el dataset en: {_datasetPath}";
                        return;
                    }
                    var dataView = _mlContext.Data.LoadFromTextFile<SentimentData>(_datasetPath, hasHeader: true);
                    var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                        .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                            labelColumnName: nameof(SentimentData.Label),
                            featureColumnName: "Features"));

                    _model = pipeline.Fit(dataView);
                    _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
                }

                if (_model != null)
                {
                    _predEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
                }
                else if (string.IsNullOrEmpty(_error)) //Only set error if not set by dataset not found
                {
                     _error = "El modelo de sentimiento no pudo ser cargado ni entrenado.";
                }
            }
            catch (Exception ex)
            {
                _error = $"Error al cargar o entrenar el modelo de sentimiento: {ex.Message}";
            }
        }

        public SentimentPrediction? Predict(string text)
        {
            if (!string.IsNullOrEmpty(_error) || _predEngine == null)
            {
                // Optionally log the error or handle it if needed before returning null
                return null;
            }
            var input = new SentimentData { Text = text };
            return _predEngine.Predict(input);
        }
    }
}
