param(
    [string]$HostUrl = "http://127.0.0.1:5001",
    [string]$Database = "edan_platform"
)

$ErrorActionPreference = "Stop"

$secure = Read-Host "PostgreSQL 비밀번호" -AsSecureString
$passwordPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPtr)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPtr)
}

if ([string]::IsNullOrWhiteSpace($password)) {
    throw "PostgreSQL 비밀번호가 비어 있습니다."
}

$connection = "Host=192.168.56.101;Port=5432;Database=$Database;Username=edan;Password=$password;Ssl Mode=Disable"
$project = Join-Path $PSScriptRoot "../../src/Hosts/EDAN.Tensor.WorkerHost/EDAN.Tensor.WorkerHost.csproj"

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Database__Provider = "PostgreSQL"
$env:Database__ApplyMigrationsInDevelopment = "false"
$env:ConnectionStrings__PlatformDatabase = $connection
$env:ConnectionStrings__WorkerDatabase = $connection
$env:ConnectionStrings__DataSetDatabase = $connection
# 콘텐츠 저장소는 현재 설계상 SQLite를 사용하므로 PostgreSQL 연결과 분리합니다.
$env:ConnectionStrings__DataSetContentDatabase = "Data Source=App_Data/edan-tensor-dataset-content.db;Cache=Shared"

try {
    dotnet run --project $project --configuration Release --no-build --urls $HostUrl
    if ($LASTEXITCODE -ne 0) {
        throw "WorkerHost 실행에 실패했습니다. 종료 코드: $LASTEXITCODE"
    }
}
finally {
    Remove-Item Env:ASPNETCORE_ENVIRONMENT, Env:Database__Provider, Env:Database__ApplyMigrationsInDevelopment, Env:ConnectionStrings__PlatformDatabase, Env:ConnectionStrings__WorkerDatabase, Env:ConnectionStrings__DataSetDatabase, Env:ConnectionStrings__DataSetContentDatabase -ErrorAction SilentlyContinue
    Remove-Variable password, connection -ErrorAction SilentlyContinue
}
