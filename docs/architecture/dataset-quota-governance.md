# DataSet Hierarchical Quota Governance

## Resolution order

The DataSet storage quota provider combines policy assignments in this order:

1. Default policy (plan/platform baseline)
2. Tenant
3. Organization
4. Project
5. DataSet

A missing value inherits the current value. A configured child limit can only tighten an inherited limit: the effective limit is the minimum of the inherited and local non-null values. This prevents a child policy from bypassing an upstream capacity boundary. Warning threshold is selected from the most-specific matching assignment.

## Configuration

```json
{
  "DataSetContent": {
    "Quota": {
      "TenantLimitBytes": 10737418240,
      "OrganizationLimitBytes": null,
      "ProjectLimitBytes": null,
      "DataSetLimitBytes": null,
      "WarningThresholdPercent": 80,
      "Assignments": [
        {
          "Scope": "Tenant",
          "TargetId": "00000000-0000-0000-0000-000000000001",
          "TenantLimitBytes": 5368709120,
          "WarningThresholdPercent": 85
        }
      ]
    }
  }
}
```

Assignments are configuration-backed in this phase. The same resolver contract is intended to be backed by persisted, audited policy records in the next phase. A null limit means inherit; an unlimited child override is intentionally not supported because it could weaken a parent boundary.
