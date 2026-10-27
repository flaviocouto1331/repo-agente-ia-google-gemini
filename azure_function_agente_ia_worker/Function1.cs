using azure_function_agente_ia_worker.Model.Request;
using azure_function_agente_ia_worker.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace azure_function_agente_ia_worker;

public class Function1
{
    private readonly ILogger<Function1> _logger;
    private readonly IConfiguration _configuracao;
    private readonly HttpClient _http;

    public Function1(ILogger<Function1> logger, IConfiguration configuracao, HttpClient http)
    {
        _logger = logger;
        _configuracao = configuracao ?? throw new ArgumentNullException(nameof(configuracao), "Configuração inválida.");
        _http = http ?? throw new ArgumentNullException(nameof(_http), "Configuração Http inválida");
    }

    [Function("function-agente-ia-google-gemini")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("function-agente-ia-google-gemini requisição recebida.");

        try
        {
            #region Requisição POST
            
            var requisicao = await req.ReadFromJsonAsync<JogosRequest>();
            
            if (requisicao is null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new { status = false, mensagem = "Function json inválido." });
                return badRequest;
            }
            
            _logger.LogInformation("Jogo NomeJogo {NomeJogo}", requisicao.NomeJogo);
            
            #endregion

            #region Configuração Google Gemini            
            
            var apiKey = _configuracao["GEMINI_API_KEY"];
            var model = "gemini-flash-latest";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var prompt = $" Atue como um assistente de cadastro. Com base no jogo '{requisicao.NomeJogo}'," +
                $" informe o gênero do jogo limitado a 200 caracteres," +
                $" plataforma do jogo limitado a 2000 caracteres e" +
                $" descrição do jogo limitado a 20000 caracteres." +
                $" Responda estritamente em formato JSON limpo," +
                $" contendo a propriedade \"GeneroJogo\" \"PlataformaJogo\" \"DescricaoJogo\" .";

            var geminiPayload = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
            };

            #endregion

            #region Requisição POST Google Gemini

            var response = await _http.PostAsJsonAsync(url, geminiPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Api Google Gemini erro: {errorBody}", errorBody);

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable) // Verifica se é um erro de alta demanda (503)
                {
                    var busyResponse = req.CreateResponse(HttpStatusCode.ServiceUnavailable);
                    await busyResponse.WriteAsJsonAsync(new { status = false, mensagem = "Agente IA Gemini está com alta demanda no momento." });
                    return busyResponse;                        
                }

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { status = false, mensagem = "Erro no Agente IA Gemini falha na comunicação." });
                return errorResponse; 
            }

            var geminiResult = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();
            var generatedText = geminiResult?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "{}";
            generatedText = generatedText.Replace("```json", "").Replace("```", "").Trim(); // Remove blocos markdown

            var jogosAgenteResponse = JsonSerializer.Deserialize<JogosAgenteResponse>(generatedText);

            #endregion

            #region Resposta POST Google Gemini com sucesso

            var okResponse = req.CreateResponse(HttpStatusCode.OK);

            //await okResponse.WriteAsJsonAsync(new
            //{
            //    GeneroJogo = jogosAgenteResponse?.GeneroJogo ?? "Não informado",
            //    PlataformaJogo = jogosAgenteResponse?.PlataformaJogo ?? "Não informado",
            //    DescricaoJogo = jogosAgenteResponse?.DescricaoJogo ?? "Não informado"
            //});
            await okResponse.WriteAsJsonAsync(jogosAgenteResponse);

            _logger.LogInformation("function-agente-ia-google-gemini requisição finalizada com sucesso.");

            return okResponse;
            
            #endregion
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Requisição inválida.");
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(new { status = false, mensagem = "Requisição inválida.", detalhe = ex.Message });
            return badResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na Function.");
            var serverError = req.CreateResponse(HttpStatusCode.InternalServerError);
            await serverError.WriteAsJsonAsync(new { status = false, mensagem = "Erro na Function.", detalhe = ex.Message });
            return serverError;
        }
    }

    public class GeminiApiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[] Candidates { get; set; } = Array.Empty<Candidate>();
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; } = new();
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; } = Array.Empty<Part>();
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}