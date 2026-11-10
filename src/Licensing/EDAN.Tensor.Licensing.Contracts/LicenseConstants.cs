namespace EDAN_Tensor_Licensing_Contracts;

/// <summary>개발 라이선스의 승인된 고정값을 제공합니다.</summary>
public static class DevelopmentLicenseConstants
{
    /// <summary>개발 라이선스의 대상 Tenant 코드입니다.</summary>
    public const string TenantCode = "EDAN-NONPROD";

    /// <summary>개발 라이선스의 제품 코드입니다.</summary>
    public const string Product = "EDAN.Tensor";

    /// <summary>개발 라이선스가 허용하는 Platform 기능입니다.</summary>
    public const string PlatformFeature = "Platform";

    /// <summary>개발 라이선스가 허용하는 DataSet 기능입니다.</summary>
    public const string DataSetFeature = "DataSet";

    /// <summary>개발 라이선스가 허용하는 Worker 기능입니다.</summary>
    public const string WorkerFeature = "Worker";

    /// <summary>개발 라이선스의 유효 기간(년)입니다.</summary>
    public const int ValidityYears = 5;

    /// <summary>개발 라이선스가 허용하는 기능을 안정적인 순서로 반환합니다.</summary>
    public static IReadOnlyList<string> Features { get; } =
        [PlatformFeature, DataSetFeature, WorkerFeature];
}

