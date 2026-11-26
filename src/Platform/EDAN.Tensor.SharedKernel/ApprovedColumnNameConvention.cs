namespace EDAN.Tensor.SharedKernel;

/// <summary>승인된 전자사전 규칙으로 물리 컬럼명을 계산합니다.</summary>
public static class ApprovedColumnNameConvention
{
    private static readonly IReadOnlyDictionary<string, string> TablePrefixes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["edpf_tnt"] = "tnt", ["edpf_org"] = "org", ["edpf_prj"] = "prj", ["edpf_usr"] = "usr",
            ["edpf_tntmbr"] = "tnt_mbr", ["edpf_orgmbr"] = "org_mbr", ["edpf_prjmbr"] = "prj_mbr",
            ["edpf_rsrc"] = "rsrc", ["edpf_plcy"] = "plcy", ["edpf_authplcy"] = "auth_plcy",
            ["edpf_authaud"] = "auth_aud", ["edpf_authplcy_chg"] = "auth_plcy_chg",
            ["edpf_authaud_rtn"] = "auth_aud_rtn", ["edpf_authaud_rtnchg"] = "auth_aud_rtn_chg",
            ["edpf_authaud_arcjob"] = "auth_aud_arc_job", ["edpf_authaud_arcattm"] = "auth_aud_arc_attm",
            ["edpf_authaud_arcschd"] = "auth_aud_arc_schd", ["edpf_strqta_plcy"] = "str_qta_plcy",
            ["edpf_strqta_plcychg"] = "str_qta_plcy_chg", ["edpf_tntdbnd"] = "tnt_dbnd",
            ["edpf_tntrtnalw"] = "tnt_rtn_alw", ["edpf_tntmut_aud"] = "tnt_mut_aud",
            ["edw_01001"] = "tnt", ["edw_01002"] = "tnt_mbr", ["edw_01003"] = "org",
            ["edw_01004"] = "usr", ["edw_01005"] = "org_mbr", ["edw_01006"] = "prj_mbr",
            ["edw_01007"] = "prj", ["edw_01008"] = "rsrc", ["edw_01009"] = "plcy",
            ["edw_01010"] = "auth_plcy", ["edw_01011"] = "auth_aud", ["edw_01012"] = "auth_plcy_chg",
            ["edw_01013"] = "auth_aud_rtn", ["edw_01014"] = "auth_aud_rtn_chg",
            ["edw_02001"] = "str_qta_plcy", ["edw_02002"] = "str_qta_plcy_chg",
            ["edw_03001"] = "auth_aud_arc_job", ["edw_03002"] = "auth_aud_arc_attm",
            ["edw_03003"] = "auth_aud_arc_schd", ["edw_04001"] = "tnt_dbnd",
            ["edw_04002"] = "tnt_rtn_alw", ["edw_04003"] = "tnt_mut_aud",
            ["edw_03004"] = "wkr_dlv", ["edw_03005"] = "wkr_tsk",
            ["edw_03006"] = "wkr_rpl", ["edw_03007"] = "wkr_typ",
            ["edw_05001"] = "dset", ["edw_05002"] = "dset_schm_rev",
            ["edw_05003"] = "dset_str_usg_snp", ["edw_05004"] = "dset_qta_thr_evt",
            ["edw_05005"] = "dset_vld_prf", ["edw_05006"] = "dset_vld_prf_rev",
            ["edw_05007"] = "dset_vld_exc", ["edw_05008"] = "dset_vld_dtl",
            ["edw_05009"] = "dset_vld_seen_key",
            ["edds_dset"] = "dset", ["edds_dsetschm_rev"] = "dset_schm_rev",
            ["edds_dsetstr_usgsnp"] = "dset_str_usg_snp", ["edds_dsetqta_threvt"] = "dset_qta_thr_evt",
            ["edds_dsetvld_prf"] = "dset_vld_prf", ["edds_dsetvld_prfrev"] = "dset_vld_prf_rev",
            ["edds_dsetvld_exc"] = "dset_vld_exc", ["edds_dsetvld_dtl"] = "dset_vld_dtl",
            ["edds_dsetvld_seenkey"] = "dset_vld_seen_key", ["eddc_dsetctt"] = "dset_ctt",
            ["edwk_wkrtsk"] = "wkr_tsk", ["edwk_wkrrpl"] = "wkr_rpl", ["edwk_wkrtyp"] = "wkr_typ",
            ["edwk_wkrdlv"] = "wkr_dlv"
        };
    private static readonly IReadOnlyDictionary<string, string> CommonNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["TenantId"] = "tntid", ["OrganizationId"] = "orgid", ["ProjectId"] = "prjid",
            ["SubjectId"] = "subid", ["UserId"] = "usrid", ["PolicyId"] = "plcyid",
            ["DataSetId"] = "dsetid", ["ExecutionId"] = "excid", ["ProfileId"] = "prfid",
            ["OriginalTaskId"] = "orig_tskid", ["ReplayTaskId"] = "rpl_tskid",
            ["TaskId"] = "tskid", ["JobId"] = "jobid", ["RequestId"] = "reqid",
            ["IdempotencyKey"] = "idem_key", ["ReplayRequestId"] = "rpl_reqid",
            ["Name"] = "nm", ["Code"] = "cd", ["Email"] = "eml", ["DisplayName"] = "dspnm",
            ["Status"] = "st", ["Role"] = "role", ["Version"] = "ver", ["Revision"] = "rev",
            ["Description"] = "desc", ["IsEnabled"] = "ena_yn", ["IsActive"] = "act_yn",
            ["CreatedAtUtc"] = "crt_utc", ["UpdatedAtUtc"] = "upd_utc", ["CompletedAtUtc"] = "cmp_utc",
            ["OccurredAtUtc"] = "occ_utc", ["CapturedAtUtc"] = "cap_utc", ["StartedAtUtc"] = "strt_utc",
            ["FinishedAtUtc"] = "fin_utc", ["NextRunAtUtc"] = "nxt_utc", ["NextAttemptAtUtc"] = "nxt_attm_utc",
            ["LastRunAtUtc"] = "last_utc", ["LeaseExpiresAtUtc"] = "lease_exp_utc",
            ["ReadyAtUtcUnixMilliseconds"] = "ready_utc_ms", ["LeaseExpiresAtUtcUnixMilliseconds"] = "lease_exp_utc_ms",
            ["CreatedAtUtcUnixMilliseconds"] = "crt_utc_ms", ["AttemptCount"] = "attm_cnt",
            ["MaxAttempts"] = "max_attm_cnt", ["ProgressPercent"] = "prgrs_pct", ["ProgressStage"] = "prgrs_stg",
            ["ProgressMessage"] = "prgrs_msg", ["ResultReference"] = "rslt_ref", ["LastErrorCode"] = "last_err_cd",
            ["TargetUri"] = "tgt_uri", ["Payload"] = "pld", ["StorageReference"] = "str_ref",
            ["Action"] = "act", ["ArchiveId"] = "arcid", ["AttemptNumber"] = "attm_num",
            ["Content"] = "ctt", ["ContentFormat"] = "ctt_fmt", ["DataSetCount"] = "data_set_count",
            ["DataSetLimitBytes"] = "dset_lmt_byt", ["DefinitionJson"] = "def_json", ["DetailItemCount"] = "dtl_item_cnt",
            ["DetailTruncated"] = "dtl_trunc_yn", ["Effect"] = "eff", ["ErrorCount"] = "err_cnt",
            ["EventType"] = "event_type", ["ExpectedDataSetVersion"] = "exp_dset_ver", ["FailureCode"] = "fail_cd",
            ["FailureMessage"] = "fail_msg", ["FailureReason"] = "fail_rsn", ["FromUtc"] = "from_utc",
            ["IntervalHours"] = "int_hr", ["IsAllowed"] = "allow_yn", ["IsLegalHold"] = "legal_hold_yn",
            ["IsSuccessful"] = "succ_yn", ["Key"] = "plcykey", ["LeaseId"] = "leaseid",
            ["LegalHoldReason"] = "legal_hold_rsn", ["LimitBytes"] = "limit_bytes", ["MatchedPolicyId"] = "plcyid",
            ["OrganizationLimitBytes"] = "org_lmt_byt", ["PayloadReference"] = "pld_ref", ["PeriodStartUtc"] = "period_strt_utc",
            ["PlanCode"] = "plan_cd", ["PolicyVersion"] = "policy_version", ["Priority"] = "pri",
            ["ProcessedRecords"] = "proc_rec_cnt", ["ProfileRevision"] = "prf_rev", ["ProjectLimitBytes"] = "prj_lmt_byt",
            ["ReasonCode"] = "rsn_cd", ["RecordCount"] = "rec_cnt", ["ResultCode"] = "result_code",
            ["RuleCountsJson"] = "rule_cnt_json", ["SchemaVersion"] = "schm_ver", ["Scope"] = "scp",
            ["Sha256"] = "sha256", ["SizeBytes"] = "size_byt", ["StorageAdapter"] = "str_adp",
            ["TargetId"] = "tgtid", ["TaskType"] = "tsktyp", ["TenantLimitBytes"] = "tnt_lmt_byt",
            ["ToUtc"] = "to_utc", ["TotalRecords"] = "tot_rec_cnt", ["UpdatedBy"] = "upd_subid",
            ["UsedBytes"] = "used_bytes", ["UtilizationPercent"] = "utilization_percent", ["Value"] = "val",
            ["WarningCount"] = "warn_cnt", ["WarningThresholdPercent"] = "warn_thr_pct",
            ["ResourceType"] = "rsrc_typ", ["SchemaJson"] = "schm_json", ["SchemaName"] = "schm_nm",
            ["StorageKind"] = "str_typ", ["StoragePath"] = "str_path", ["RetentionDays"] = "rtn_day",
            ["ActorSubjectId"] = "act_subid", ["ChangeType"] = "chg_typ", ["BeforeJson"] = "bef_json",
            ["AfterJson"] = "aft_json", ["ErrorCode"] = "err_cd", ["ErrorMessage"] = "err_msg",
            ["Checkpoint"] = "ckpt", ["CheckpointVersion"] = "ckpt_ver", ["FencingToken"] = "fence_tkn",
            ["WorkerId"] = "wkrid", ["DetailReference"] = "dtl_ref", ["FieldName"] = "fldnm",
            ["RowNumber"] = "row_num", ["RuleCode"] = "rule_cd", ["KeyHash"] = "key_hash",
            ["Severity"] = "sev", ["Message"] = "msg", ["Provider"] = "prvdr", ["DatabaseName"] = "db_nm",
            ["ServerAddress"] = "svr_addr", ["Port"] = "port", ["Username"] = "usrnm", ["SecretKey"] = "scrt_key"
        };

    public static string? For(string tableName, string propertyName)
    {
        // Sequential framework tables use the table number as the physical
        // prefix for their primary/name/code columns (for example 05001id).
        if (tableName.StartsWith("edw_", StringComparison.Ordinal)
            && tableName.Length == 9
            && int.TryParse(tableName.AsSpan(4), out _)
            && propertyName is "Id" or "Name" or "Code")
        {
            return tableName[4..] + (propertyName switch
            {
                "Id" => "id",
                "Name" => "nm",
                _ => "cd"
            });
        }

        var prefix = TablePrefix(tableName);
        if (propertyName == "Id")
            return prefix + "id";
        if (propertyName is "Name" or "Code")
            return prefix + (propertyName == "Name" ? "nm" : "cd");
        return CommonNames.TryGetValue(propertyName, out var name) ? name : null;
    }

    private static string TablePrefix(string tableName)
    {
        return TablePrefixes.TryGetValue(tableName, out var prefix)
            ? prefix
            : tableName[(tableName.IndexOf('_') + 1)..].Replace("_", string.Empty, StringComparison.Ordinal);
    }
}
