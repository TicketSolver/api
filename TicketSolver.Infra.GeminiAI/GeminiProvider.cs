using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TicketSolver.Application.Ports;

namespace TicketSolver.Infra.GeminiAI
{
    public class GeminiProvider : IAiProvider
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public GeminiProvider(HttpClient http, IConfiguration cfg)
        {
            _http = http;

            // Base URL
            var baseUrl = cfg["Ai:Groq:BaseUrl"]
                          ?? Environment.GetEnvironmentVariable("Ai__Groq__BaseUrl")
                          ?? throw new InvalidOperationException("Ai:Groq:BaseUrl não configurada");
            _http.BaseAddress = new Uri(baseUrl);

            // API Key (tenta JSON, depois .env)
            var apiKey = cfg["Ai:Groq:ApiKey"]
                         ?? Environment.GetEnvironmentVariable("Ai__Groq__ApiKey")
                         ?? throw new InvalidOperationException("Ai:Groq:ApiKey não configurada");
            _http.DefaultRequestHeaders.Add("x-api-key", apiKey);

            // Modelo
            _model = cfg["Ai:Groq:Model"]
                     ?? Environment.GetEnvironmentVariable("Ai__Groq__Model")
                     ?? throw new InvalidOperationException("Ai:Groq:Model não configurado");
        }

        public async Task<string> GenerateTextAsync(string prompt, CancellationToken ct)
        {
            var body = new
            {
                model = _model,
                prompt = prompt,
                max_tokens_to_sample = 512,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync("complete", content, ct);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            return doc.RootElement.GetProperty("completion").GetString() ?? string.Empty;
        }

        public Task<byte[]> GenerateBinaryAsync(object payload, CancellationToken ct)
            => throw new NotImplementedException("GroqProvider só implementa texto por enquanto");
    }
}
