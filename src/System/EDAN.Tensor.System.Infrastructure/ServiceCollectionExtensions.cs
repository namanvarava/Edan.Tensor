using EDAN.Tensor.System.Application;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.System.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemModule(this IServiceCollection services) => services.AddSingleton<ISystemMenuService, InMemorySystemMenuService>();
}
