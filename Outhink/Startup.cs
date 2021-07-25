using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Outhink.Db.Context;
using Outhink.Db.Enums;
using Outhink.Db.Models;
using Outhink.Db.Repositories;
using System.Collections.Generic;
using System.Reflection;

namespace Outhink
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        readonly string CorsOrigin = "OuthinkCors";

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: CorsOrigin,
                                  builder =>
                                  {
                                      builder.WithOrigins("https://localhost:4200", "http://localhost:4200")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                                  });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Outhink", Version = "v1" });
            });

            services.AddDbContext<OuthinkContext>(opt => opt.UseInMemoryDatabase(nameof(Outhink)));

            #region Auto Mapper

            services.AddAutoMapper(typeof(Startup));

            #endregion

            #region Add In Memory Caching

            services.AddMemoryCache();

            #endregion

            #region Dependency Injection

            InjectServices(services);

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Outhink v1"));
            }

            app.UseCors(CorsOrigin);

            #region Adding In Memory Data

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<OuthinkContext>();
                AddItems(context);
                AddCoins(context);
            }

            #endregion

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #region Private Methods

        /// <summary>
        /// Method used to inject the services
        /// </summary>
        /// <param name="services"></param>
        private static void InjectServices(IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Method used to add the in memory vending machine items
        /// </summary>
        /// <param name="context">Outhink context</param>
        private static void AddItems(OuthinkContext context)
        {
            Item teaItem = new()
            {
                Name = "Tea",
                Quantity = 10,
                Price = 1.3
            };

            Item espressoItem = new()
            {
                Name = "Espresso",
                Quantity = 20,
                Price = 1.8
            };

            Item juiceItem = new()
            {
                Name = "Juice",
                Quantity = 20,
                Price = 1.8
            };

            Item chickenSoupItem = new()
            {
                Name = "Chicken Soup",
                Quantity = 15,
                Price = 1.8
            };

            List<Item> items = new()
            {
                teaItem,
                espressoItem,
                juiceItem,
                chickenSoupItem
            };

            context.Items.AddRange(items);
            context.SaveChanges();
        }

        /// <summary>
        /// Method used to add the in memory vending machine coins
        /// </summary>
        /// <param name="context">Outhink context</param>
        private static void AddCoins(OuthinkContext context)
        {
            Coin tenCents = new()
            {
                Type = CoinType.TenCent,
                Quantity = 100
            };

            Coin twentyCents = new()
            {
                Type = CoinType.TwentyCent,
                Quantity = 100
            };

            Coin fiftyCents = new()
            {
                Type = CoinType.FiftyCent,
                Quantity = 100
            };

            Coin oneEuro = new()
            {
                Type = CoinType.OneEuro,
                Quantity = 100
            };

            List<Coin> coins = new()
            {
                tenCents,
                twentyCents,
                fiftyCents,
                oneEuro
            };

            context.Coins.AddRange(coins);
            context.SaveChanges();
        }

        #endregion
    }
}
