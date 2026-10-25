using cad_jogo_api.Model.Entities;

namespace cad_jogo_api.Data.Repositories
{
    public interface IJogosRepository
    {
        Task<List<JogosEntity>> PegarTodosJogos();
        Task<JogosEntity> CriarJogo(JogosCriarEntity jogo);
    }
}
