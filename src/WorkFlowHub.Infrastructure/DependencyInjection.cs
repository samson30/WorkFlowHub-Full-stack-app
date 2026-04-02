using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFlowHub.Core.Interfaces.Repositories;
using WorkFlowHub.Core.Interfaces.Services;
using WorkFlowHub.Infrastructure.Data;
using WorkFlowHub.Infrastructure.Repositories;
using WorkFlowHub.Infrastructure.Services;

namespace WorkFlowHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("WorkFlowHub.Infrastructure")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IFileRepository, FileRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
