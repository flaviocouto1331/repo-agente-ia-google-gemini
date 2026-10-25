using cad_jogo_api.Data.Repositories;
using cad_jogo_api.Services;

namespace cad_jogo_api.Extension
{
    public static class JogosExtension
    {
        public static IServiceCollection AddJogosExtension(this IServiceCollection services) 
        {
            services.AddScoped<IJogosRepository, JogosRepository>();
            services.AddTransient<IJogosService, JogosService>();
            return services;
        }
    }
}
