using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EDAN.Tensor.Edan.Infrastructure.Persistence;

/// <summary>고객사 업무 데이터베이스 Edan의 SYS/BAS 경계를 관리합니다.</summary>
public sealed class EdanDbContext(DbContextOptions<EdanDbContext> options) : DbContext(options)
{
    public DbSet<SysMenu> Menus => Set<SysMenu>();
    public DbSet<SysButton> Buttons => Set<SysButton>();
    public DbSet<SysRole> Roles => Set<SysRole>();
    public DbSet<SysRolePermission> RolePermissions => Set<SysRolePermission>();
    public DbSet<SysCommonCode> CommonCodes => Set<SysCommonCode>();
    public DbSet<BasCompany> Companies => Set<BasCompany>();
    public DbSet<BasFactory> Factories => Set<BasFactory>();
    public DbSet<BasProcess> Processes => Set<BasProcess>();
    public DbSet<BasLine> Lines => Set<BasLine>();
    public DbSet<BasEquipment> Equipments => Set<BasEquipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysMenu>(e =>
        {
            e.ToTable("ed_00001", t => t.HasComment("시스템 메뉴"));
            e.HasKey(x => x.Id).HasName("PK_ed_00001");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.ParentId);
            e.Property(x => x.Route).HasMaxLength(300);
            e.Property(x => x.SortOrder).IsRequired();
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_ed_00001_code");
            e.HasIndex(x => x.ParentId).HasDatabaseName("IX_ed_00001_parent_id");
        });
        modelBuilder.Entity<SysButton>(e =>
        {
            e.ToTable("ed_00002", t => t.HasComment("메뉴 버튼 및 권한 대상"));
            e.HasKey(x => x.Id).HasName("PK_ed_00002");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.MenuId).IsRequired();
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.HasIndex(x => new { x.MenuId, x.Code }).IsUnique().HasDatabaseName("UX_ed_00002_menu_code");
            e.HasOne<SysMenu>().WithMany().HasForeignKey(x => x.MenuId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ed_00002_00001_menu_id");
        });
        modelBuilder.Entity<SysRole>(e =>
        {
            e.ToTable("ed_00003", t => t.HasComment("고객사 시스템 역할"));
            e.HasKey(x => x.Id).HasName("PK_ed_00003");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_ed_00003_code");
        });
        modelBuilder.Entity<SysRolePermission>(e =>
        {
            e.ToTable("ed_00004", t => t.HasComment("역할별 메뉴 버튼 권한"));
            e.HasKey(x => new { x.RoleId, x.ButtonId }).HasName("PK_ed_00004");
            e.HasOne<SysRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ed_00004_00003_role_id");
            e.HasOne<SysButton>().WithMany().HasForeignKey(x => x.ButtonId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ed_00004_00002_button_id");
        });
        modelBuilder.Entity<SysCommonCode>(e =>
        {
            e.ToTable("ed_00005", t => t.HasComment("공통 코드 및 코드 일람표"));
            e.HasKey(x => x.Id).HasName("PK_ed_00005");
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.GroupCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.HasIndex(x => new { x.GroupCode, x.Code }).IsUnique().HasDatabaseName("UX_ed_00005_group_code");
        });
        ConfigureBase(e => e.ToTable("ed_01001", t => t.HasComment("고객사")), modelBuilder.Entity<BasCompany>(), "PK_ed_01001");
        ConfigureBase(e => e.ToTable("ed_01002", t => t.HasComment("공장")), modelBuilder.Entity<BasFactory>(), "PK_ed_01002");
        ConfigureBase(e => e.ToTable("ed_01003", t => t.HasComment("공정")), modelBuilder.Entity<BasProcess>(), "PK_ed_01003");
        ConfigureBase(e => e.ToTable("ed_01004", t => t.HasComment("생산 라인")), modelBuilder.Entity<BasLine>(), "PK_ed_01004");
        ConfigureBase(e => e.ToTable("ed_01005", t => t.HasComment("설비")), modelBuilder.Entity<BasEquipment>(), "PK_ed_01005");
        var factory = modelBuilder.Entity<BasFactory>();
        factory.HasOne<BasCompany>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_ed_01002_01001_company_id");
        var process = modelBuilder.Entity<BasProcess>();
        process.HasOne<BasFactory>().WithMany().HasForeignKey(x => x.FactoryId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_ed_01003_01002_factory_id");
        var line = modelBuilder.Entity<BasLine>();
        line.HasOne<BasProcess>().WithMany().HasForeignKey(x => x.ProcessId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_ed_01004_01003_process_id");
        var equipment = modelBuilder.Entity<BasEquipment>();
        equipment.HasOne<BasLine>().WithMany().HasForeignKey(x => x.LineId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("FK_ed_01005_01004_line_id");
    }

    private static void ConfigureBase<TEntity>(Action<EntityTypeBuilder<TEntity>> table, EntityTypeBuilder<TEntity> entity, string keyName)
        where TEntity : BasNamedEntity
    {
        table(entity);
        entity.HasKey(x => x.Id).HasName(keyName);
        entity.Property(x => x.Id).ValueGeneratedNever();
        entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
        entity.HasIndex(x => x.Code).IsUnique();
    }
}

public abstract class BasNamedEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
public sealed class SysMenu { public Guid Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; public Guid? ParentId { get; set; } public string? Route { get; set; } public int SortOrder { get; set; } }
public sealed class SysButton { public Guid Id { get; set; } public Guid MenuId { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
public sealed class SysRole { public Guid Id { get; set; } public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
public sealed class SysRolePermission { public Guid RoleId { get; set; } public Guid ButtonId { get; set; } }
public sealed class SysCommonCode { public Guid Id { get; set; } public string GroupCode { get; set; } = ""; public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
public sealed class BasCompany : BasNamedEntity { }
public sealed class BasFactory : BasNamedEntity { public Guid CompanyId { get; set; } }
public sealed class BasProcess : BasNamedEntity { public Guid FactoryId { get; set; } }
public sealed class BasLine : BasNamedEntity { public Guid ProcessId { get; set; } }
public sealed class BasEquipment : BasNamedEntity { public Guid LineId { get; set; } }
