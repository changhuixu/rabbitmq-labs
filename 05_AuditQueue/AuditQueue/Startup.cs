using System;
using AuditQueue.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace AuditQueue
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var mongoHostName = Environment.GetEnvironmentVariable("MONGO_HOSTNAME") ?? "localhost";
            services.AddSingleton<IMongoOption>(new MongoOption
            {
                ConnectionString = $"mongodb://{mongoHostName}:27017",
                DatabaseName = "MessageQueue",
                MessagesCollectionName = "Messages"
            });
            services.AddSingleton<IMessagesService, MessagesService>();

            var rabbitHostName = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME") ?? "localhost";
            var connectionFactory = new ConnectionFactory
            {
                HostName = rabbitHostName,
                Port = 5672,
                UserName = "ops0",
                Password = "ops0",
                DispatchConsumersAsync = true
            };
            var connection = connectionFactory.CreateConnection();
            services.AddSingleton(connection);

            services.AddHostedService<AuditQueueService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuditQueue", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./swagger/v1/swagger.json", "AuditQueue API V1");
                c.DocumentTitle = "AuditQueue API";
                c.RoutePrefix = string.Empty;
            });
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
