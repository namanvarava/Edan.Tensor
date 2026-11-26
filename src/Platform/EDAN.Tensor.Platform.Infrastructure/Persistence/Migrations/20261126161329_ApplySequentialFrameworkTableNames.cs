using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDAN.Tensor.Platform.Infrastructure.Persistence.Migrations;

/// <summary>SQLite compatibility migration that changes only physical table names.</summary>
public partial class ApplySequentialFrameworkTableNames : Migration
{
    private static readonly (string OldName, string NewName)[] TableNames =
    [
        ("edpf_usr", "edw_01004"), ("edpf_tntrtnalw", "edw_04002"),
        ("edpf_tntmut_aud", "edw_04003"), ("edpf_tntmbr", "edw_01002"),
        ("edpf_tntdbnd", "edw_04001"), ("edpf_tnt", "edw_01001"),
        ("edpf_strqta_plcychg", "edw_02002"), ("edpf_strqta_plcy", "edw_02001"),
        ("edpf_rsrc", "edw_01008"), ("edpf_prjmbr", "edw_01006"),
        ("edpf_prj", "edw_01007"), ("edpf_plcy", "edw_01009"),
        ("edpf_orgmbr", "edw_01005"), ("edpf_org", "edw_01003"),
        ("edpf_authplcy_chg", "edw_01012"), ("edpf_authplcy", "edw_01010"),
        ("edpf_authaud_rtnchg", "edw_01014"), ("edpf_authaud_rtn", "edw_01013"),
        ("edpf_authaud_arcschd", "edw_03003"), ("edpf_authaud_arcjob", "edw_03001"),
        ("edpf_authaud_arcattm", "edw_03002"), ("edpf_authaud", "edw_01011")
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var (oldName, newName) in TableNames)
            migrationBuilder.Sql($"ALTER TABLE \"{oldName}\" RENAME TO \"{newName}\";");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        for (var i = TableNames.Length - 1; i >= 0; i--)
            migrationBuilder.Sql($"ALTER TABLE \"{TableNames[i].NewName}\" RENAME TO \"{TableNames[i].OldName}\";");
    }
}
