using cad_jogo_api.Model.Request;
using cad_jogo_api.Model.Response;
using cad_jogo_api.Services;

namespace cad_jogo_api.Endpoint
{
    public static class JogosEndpoint
    {
        public static void AddJogosEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/jogos", async (IJogosService _service) => {
                try
                {
                    var jogos = await _service.PegarTodosJogos();

                    if (jogos.Count == 0) return Results.Ok(new { status = true, mensagem = "Nenhum registro encontrado.", Jogos = new List<JogosResponse>() });

                    return Results.Ok(new { status = true, mensagem = "Registro encontrado.", Jogos = jogos });
                }
                catch (ArgumentException ex) 
                {
                    return Results.BadRequest(new { status = false, mensagem = "Requisição inválida.", detalhe = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { status = false, mensagem = "Erro na Api.", detalhe = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
                }
            }).WithTags("Jogos");

            app.MapPost("/api/Jogos", async (JogosRequest _jogo, IJogosService _service) => {
                try
                {
                    if (_jogo is null) throw new ArgumentNullException(nameof(_jogo), "Jogo inválido.");
                    
                    #region Azure Function 

                    //TODO - falta o desenvolvimento 
                    var complementoJogo = new JogosAgenteResponse();

                    #endregion

                    var jogo = await _service.CriarJogo(_jogo, complementoJogo);

                    return Results.Ok(new { status = true, mensagem = "Registro inserido com sucesso.", Jogos = jogo });
                }
                catch (ArgumentException ex) 
                {
                    return Results.BadRequest(new { status = false, mensagem = "Requisição inválida.", detalhe = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { status = false, mensagem = "Erro na Api", detalhe = ex.Message }, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithTags("Jogos")
            .WithDescription("Agente de IA Google Gemini\n\nPasso 1 - enviar o nome do jogo\n\nPasso 2 - o agente de ia do google gemini irá inserir os dados restantes sobre o jogo");
        }
    }
}
