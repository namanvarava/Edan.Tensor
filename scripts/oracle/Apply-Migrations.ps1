param(
    [switch]$Apply,
    [ValidateSet("PlatformDbContext", "WorkerDbContext", "DataSetDbContext")]
    [string[]]$Contexts = @("PlatformDbContext", "WorkerDbContext", "DataSetDbContext"),
    [string]$Server = "127.0.0.1",
    [int]$Port = 1521,
    [string]$Service = "FREEPDB1",
    [string]$User = "EDAN"
)

$ErrorActionPreference = "Stop"
$secure = Read-Host "Oracle 비밀번호" -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try { $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
if ([string]::IsNullOrWhiteSpace($password)) { throw "Oracle 비밀번호가 비어 있습니다." }

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "../..")
$startupProject = Join-Path $repoRoot "src/Hosts/EDAN.Tensor.ApiHost/EDAN.Tensor.ApiHost.csproj"
$migrationProject = Join-Path $repoRoot "src/Hosts/EDAN.Tensor.OracleMigrations/EDAN.Tensor.OracleMigrations.csproj"
$connection = "User Id=$User;Password=$password;Data Source=$Server`:$Port/$Service"
$env:Database__Provider = "Oracle"
try {
    dotnet build $startupProject --configuration Release --disable-build-servers -m:1 --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Oracle Migration 산출물 빌드에 실패했습니다. 종료 코드: $LASTEXITCODE" }
    foreach ($context in $Contexts) {
        $variable = switch ($context) {
            "PlatformDbContext" { "ConnectionStrings__PlatformDatabase" }
            "WorkerDbContext" { "ConnectionStrings__WorkerDatabase" }
            "DataSetDbContext" { "ConnectionStrings__DataSetDatabase" }
        }
        [Environment]::SetEnvironmentVariable($variable, $connection, "Process")
        if ($Apply) { dotnet ef database update --project $migrationProject --startup-project $startupProject --context $context --configuration Release --no-build }
        else {
            $output = Join-Path $env:TEMP "edan-$context-oracle.sql"
            dotnet ef migrations script --project $migrationProject --startup-project $startupProject --context $context --configuration Release --no-build --idempotent --output $output
            Write-Host "Migration 적용 미리보기: $output"
        }
        if ($LASTEXITCODE -ne 0) { throw "$context Oracle Migration 명령이 실패했습니다. 종료 코드: $LASTEXITCODE" }
    }
}
finally {
    Remove-Item Env:Database__Provider, Env:ConnectionStrings__PlatformDatabase, Env:ConnectionStrings__WorkerDatabase, Env:ConnectionStrings__DataSetDatabase -ErrorAction SilentlyContinue
    Remove-Variable password, connection -ErrorAction SilentlyContinue
}
