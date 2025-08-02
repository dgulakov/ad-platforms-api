using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using AdPlatformsApi.Model;
using AdPlatformsApi.Handlers;

namespace AdPlatformsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IAdPlatformsCollection, AdPlatformsCollection>();
            builder.Services.AddSingleton<AdPlatformsRepository>();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            app.UseExceptionHandler(_ => { });
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
