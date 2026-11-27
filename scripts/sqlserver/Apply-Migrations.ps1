param(
    [switch]$Apply,
    [ValidateSet("PlatformDbContext", "WorkerDbContext", "DataSetDbContext")]
    [string[]]$Contexts = @("PlatformDbContext", "WorkerDbContext", "DataSetDbContext"),
    [string]$Server = "192.168.56.101",
    [int]$Port = 1433,
    [string]$User = "edan",
    [string]$PlatformDatabase = "edan_framework",
    [string]$WorkerDatabase = "edan_framework",
    [string]$DataSetDatabase = "edan_framework"
)

$ErrorActionPreference = "Stop"
$dotnetToolPath = Join-Path $HOME ".dotnet/tools"
if (Test-Path (Join-Path $dotnetToolPath "dotnet-ef")) { $env:PATH = "$dotnetToolPath$([IO.Path]::PathSeparator)$env:PATH" }
if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) { throw "dotnet-ef를 찾을 수 없습니다. 전역 dotnet-ef 도구를 설치하세요." }
$secure = Read-Host "SQL Server 비밀번호" -AsSecureString
$passwordPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try { $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPtr) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPtr) }
if ([string]::IsNullOrWhiteSpace($password)) { throw "SQL Server 비밀번호가 비어 있습니다." }

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "../..")
$startupProject = Join-Path $repoRoot "src/Hosts/EDAN.Tensor.ApiHost/EDAN.Tensor.ApiHost.csproj"
$migrationProject = Join-Path $repoRoot "src/Hosts/EDAN.Tensor.SqlServerMigrations/EDAN.Tensor.SqlServerMigrations.csproj"
$connections = @{
    PlatformDbContext = "Server=$Server,$Port;Database=$PlatformDatabase;User Id=$User;Password=$password;Encrypt=False;TrustServerCertificate=True"
    WorkerDbContext = "Server=$Server,$Port;Database=$WorkerDatabase;User Id=$User;Password=$password;Encrypt=False;TrustServerCertificate=True"
    DataSetDbContext = "Server=$Server,$Port;Database=$DataSetDatabase;User Id=$User;Password=$password;Encrypt=False;TrustServerCertificate=True"
}

$env:Database__Provider = "SqlServer"
try {
    dotnet build $startupProject --configuration Release --disable-build-servers -m:1 --no-restore
    if ($LASTEXITCODE -ne 0) { throw "SQL Server Migration 실행 산출물 빌드에 실패했습니다. 종료 코드: $LASTEXITCODE" }

    foreach ($context in $Contexts) {
        $variable = switch ($context) {
            "PlatformDbContext" { "ConnectionStrings__PlatformDatabase" }
            "WorkerDbContext" { "ConnectionStrings__WorkerDatabase" }
            "DataSetDbContext" { "ConnectionStrings__DataSetDatabase" }
        }
        [Environment]::SetEnvironmentVariable($variable, $connections[$context], "Process")
        if ($Apply) {
            Write-Host "$context Migration을 SQL Server에 적용합니다."
            dotnet ef database update --project $migrationProject --startup-project $startupProject --context $context --configuration Release --no-build
        }
        else {
            $output = Join-Path $env:TEMP "edan-$context-sqlserver.sql"
            Write-Host "$context Migration 적용 미리보기: $output"
            dotnet ef migrations script --project $migrationProject --startup-project $startupProject --context $context --configuration Release --no-build --idempotent --output $output
        }
        if ($LASTEXITCODE -ne 0) { throw "$context Migration 명령이 실패했습니다. 종료 코드: $LASTEXITCODE" }
    }
}
finally {
    Remove-Item Env:Database__Provider, Env:ConnectionStrings__PlatformDatabase, Env:ConnectionStrings__WorkerDatabase, Env:ConnectionStrings__DataSetDatabase -ErrorAction SilentlyContinue
    Remove-Variable password, connections -ErrorAction SilentlyContinue
}
