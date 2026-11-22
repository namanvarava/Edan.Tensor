param(
    [switch]$Apply,
    [ValidateSet("PlatformDbContext", "WorkerDbContext", "DataSetDbContext")]
    [string[]]$Contexts = @("PlatformDbContext", "WorkerDbContext", "DataSetDbContext"),
    [string]$PlatformDatabase = "edan_platform",
    [string]$WorkerDatabase = "edan_worker",
    [string]$DataSetDatabase = "edan_dataset"
)

$ErrorActionPreference = "Stop"

function Read-PostgreSqlPassword {
    $secure = Read-Host "PostgreSQL 비밀번호" -AsSecureString
    $passwordPtr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPtr)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPtr)
    }

    if ([string]::IsNullOrWhiteSpace($password)) {
        throw "PostgreSQL 비밀번호가 비어 있습니다. 비밀번호를 입력한 뒤 다시 실행하세요. 입력 중 문자는 화면에 표시되지 않습니다."
    }

    return $password
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "../..")
$startupProject = Join-Path $repoRoot "src/Hosts/EDAN.Tensor.ApiHost/EDAN.Tensor.ApiHost.csproj"
$migrationProject = Join-Path $repoRoot "src/Hosts/EDAN.Tensor.PostgreSqlMigrations/EDAN.Tensor.PostgreSqlMigrations.csproj"
$migrationProjects = @{
    # PostgreSQL migrations are provider-specific and are hosted in the shared migration assembly.
    PlatformDbContext = $migrationProject
    WorkerDbContext = $migrationProject
    DataSetDbContext = $migrationProject
}
$password = Read-PostgreSqlPassword
$connections = @{
    PlatformDbContext = "Host=192.168.56.101;Port=5432;Database=$PlatformDatabase;Username=edan;Password=$password;Ssl Mode=Disable"
    WorkerDbContext = "Host=192.168.56.101;Port=5432;Database=$WorkerDatabase;Username=edan;Password=$password;Ssl Mode=Disable"
    DataSetDbContext = "Host=192.168.56.101;Port=5432;Database=$DataSetDatabase;Username=edan;Password=$password;Ssl Mode=Disable"
}

$env:Database__Provider = "PostgreSQL"

try {
    Write-Host "PostgreSQL Migration 실행 산출물을 Release 구성으로 갱신합니다."
    dotnet build $startupProject `
        --configuration Release `
        --disable-build-servers `
        -m:1 `
        --no-restore

    if ($LASTEXITCODE -ne 0) {
        throw "PostgreSQL Migration 실행 산출물 빌드에 실패했습니다. 종료 코드: $LASTEXITCODE"
    }

    foreach ($context in $Contexts) {
        $project = $migrationProjects[$context]
        $connectionVariable = switch ($context) {
            "PlatformDbContext" { "ConnectionStrings__PlatformDatabase" }
            "WorkerDbContext" { "ConnectionStrings__WorkerDatabase" }
            "DataSetDbContext" { "ConnectionStrings__DataSetDatabase" }
        }
        [Environment]::SetEnvironmentVariable($connectionVariable, $connections[$context], "Process")
        if ($Apply) {
            Write-Host "$context Migration을 PostgreSQL에 적용합니다."
            dotnet ef database update `
                --project $project `
                --startup-project $startupProject `
                --context $context `
                --configuration Release `
                --no-build
        }
        else {
            Write-Host "$context Migration 적용 미리보기입니다. 실제 변경은 수행하지 않습니다."
            dotnet ef migrations script `
                --project $project `
                --startup-project $startupProject `
                --context $context `
                --configuration Release `
                --no-build `
                --idempotent `
                --output (Join-Path $env:TEMP "edan-$context-postgresql.sql")
        }

        if ($LASTEXITCODE -ne 0) {
            throw "$context Migration 명령이 실패했습니다. 종료 코드: $LASTEXITCODE"
        }
    }
}
finally {
    Remove-Item Env:Database__Provider, Env:ConnectionStrings__PlatformDatabase, Env:ConnectionStrings__WorkerDatabase, Env:ConnectionStrings__DataSetDatabase -ErrorAction SilentlyContinue
    Remove-Variable password, connections -ErrorAction SilentlyContinue
}

