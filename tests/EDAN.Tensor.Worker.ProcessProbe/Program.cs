using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// 검증 프로세스는 테스트가 생성한 전용 DB에서 Lease와 체크포인트를 영속화한 뒤 강제 종료를 기다립니다.
// 이 프로그램은 운영 WorkerHost를 대체하지 않으며 CI의 프로세스 장애 복구 시험에서만 사용합니다.
if (args.Length != 3)
    throw new ArgumentException("검증 프로세스는 DB 경로, Tenant ID 및 작업 ID를 받아야 합니다.");

var tenantId = Guid.Parse(args[1]);
var taskId = Guid.Parse(args[2]);
var connection = new SqliteConnectionStringBuilder
{
    DataSource = args[0],
    Pooling = false
}.ConnectionString;
var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
await using var db = new WorkerDbContext(options);
var repository = new WorkerTaskRepository(db);
var now = DateTimeOffset.UtcNow;
var leaseId = Guid.NewGuid();
var leased = await repository.TryAcquireLeaseAsync(
    tenantId, taskId, "장애-검증-프로세스", leaseId, now, now.AddSeconds(30))
    ?? throw new InvalidOperationException("검증 프로세스가 Lease를 획득하지 못했습니다.");
if (!await repository.TryStartAsync(tenantId, taskId, leaseId, leased.FencingToken, now))
    throw new InvalidOperationException("검증 프로세스가 작업을 시작하지 못했습니다.");
if (!await repository.TrySaveCheckpointAsync(
    tenantId, taskId, leaseId, leased.FencingToken, "offset:42", now))
    throw new InvalidOperationException("검증 프로세스가 체크포인트를 저장하지 못했습니다.");

// 준비 신호는 모든 영속 쓰기가 성공한 뒤에만 출력되며 부모 프로세스는 이 신호를 받은 다음 자식을 종료합니다.
Console.WriteLine("체크포인트-저장-완료");
await Console.Out.FlushAsync();
await Task.Delay(Timeout.InfiniteTimeSpan);
