using cad_jogo_api.Data.Repositories;
using cad_jogo_api.Model.Entities;
using cad_jogo_api.Model.Request;
using cad_jogo_api.Model.Response;

namespace cad_jogo_api.Services
{
    public class JogosService : IJogosService
    {
        private readonly IJogosRepository _repository;
        public JogosService(IJogosRepository repository) => _repository = repository;

        public async Task<JogosResponse> CriarJogo(JogosRequest jogo, JogosAgenteResponse complementoJogo)
        {
            var mapJogosCriarEntity = new JogosCriarEntity(
                jogo.NomeJogo,
                complementoJogo.GeneroJogo,
                complementoJogo.PlataformaJogo,
                complementoJogo.DescricaoJogo
                ){ };

            var jogosEntity = await _repository.CriarJogo(mapJogosCriarEntity);

            var mapJogosResponse = new JogosResponse()
            {
                Id = jogosEntity.Id,
                DataCadstro = jogosEntity.DataCadstro,
                StatusCadastro = jogosEntity.StatusCadastro,
                NomeJogo = jogosEntity.NomeJogo,
                GeneroJogo = jogosEntity.GeneroJogo,
                PlataformaJogo = jogosEntity.PlataformaJogo,
                DescricaoJogo = jogosEntity.DescricaoJogo
            };

            return mapJogosResponse;
        }

        public async Task<List<JogosResponse>> PegarTodosJogos()
        {
            var jogosEntity = await _repository.PegarTodosJogos();

            if (jogosEntity.Count == 0) return [];

            var mapJogosResponse = jogosEntity.Select(i => new JogosResponse()
            {
                Id = i.Id,
                DataCadstro = i.DataCadstro,
                StatusCadastro = i.StatusCadastro,
                NomeJogo = i.NomeJogo,
                GeneroJogo = i.GeneroJogo,
                PlataformaJogo = i.PlataformaJogo,
                DescricaoJogo = i.DescricaoJogo
            }).ToList();

            return mapJogosResponse;
        }
    }
}
