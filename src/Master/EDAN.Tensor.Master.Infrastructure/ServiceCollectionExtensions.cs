using EDAN.Tensor.Master.Application;
using Microsoft.Extensions.DependencyInjection;
namespace EDAN.Tensor.Master.Infrastructure;
public static class ServiceCollectionExtensions
{ public static IServiceCollection AddMasterModule(this IServiceCollection services) => services.AddSingleton<IMasterCodeService, InMemoryMasterCodeService>(); }
