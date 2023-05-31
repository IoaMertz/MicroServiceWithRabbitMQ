using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Infra.IoC;
using MicroRabbit.Transfer.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MicroRabbit.Transfer.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<TransferDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("TransferDbConnection")
                    );
            }
           );
            builder.Services.AddDbContext<BankingDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("BankingDbConnection")
                    );
            }
         );
            var kati = builder.Configuration;
            builder.Services.RegisterServices();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}