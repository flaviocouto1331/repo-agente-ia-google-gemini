using cad_jogo_api.Data.Dapper;

namespace cad_jogo_api.Extension
{
    public static class DapperExtension
    {
        public static IServiceCollection AddDapperExtension(this IServiceCollection services) 
        {
            services.AddScoped<DapperContext>();
            return services;
        }
    }
}
