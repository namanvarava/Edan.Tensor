using EDAN.Tensor.DataSet.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.DataSet.Application.DependencyInjection;

/// <summary>DataSet 응용 서비스를 DI 컨테이너에 등록합니다.</summary>
public static class DataSetApplicationExtensions
{
    public static IServiceCollection AddDataSetApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<DataSetManagementService>();
        services.AddScoped<DataSetValidationService>();
        return services;
    }
}
