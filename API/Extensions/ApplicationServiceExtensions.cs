using Application.Activities;
using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddDbContext<DataContext>(opt =>
                opt.UseSqlite(config.GetConnectionString("DefaultConnection")));

            services.AddCors(opt =>
                opt.AddPolicy("CorsPolicy", policy =>
                    policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000")));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssemblyContaining<ActivityValidator>();
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);

            return services;
        }
    }
}
