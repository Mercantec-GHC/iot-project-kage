using IotProject.API.Data;
using IotProject.API.Services;
using Microsoft.EntityFrameworkCore;

namespace IotProject.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGenWithAuth();

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(
                options => options.UseNpgsql(
                    Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING") ?? builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddHostedService<CleanUpServices>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
