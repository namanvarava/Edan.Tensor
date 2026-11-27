using EDAN.Tensor.Business.Application;
using Microsoft.Extensions.DependencyInjection;
namespace EDAN.Tensor.Business.Infrastructure;
public static class ServiceCollectionExtensions
{ public static IServiceCollection AddBusinessModule(this IServiceCollection services) => services.AddSingleton<IBusinessWorkItemService, InMemoryBusinessWorkItemService>(); }
