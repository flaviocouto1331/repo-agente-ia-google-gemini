using cad_jogo_api.Model.Request;
using cad_jogo_api.Model.Response;

namespace cad_jogo_api.Services
{
    public interface IJogosService
    {
        Task<List<JogosResponse>> PegarTodosJogos();
        Task<JogosResponse> CriarJogo(JogosRequest jogo, JogosAgenteResponse complementoJogo);
    }
}
