/*
ParkMate primary keys are SQL Server IDENTITY columns.
Do NOT run a blanket DBCC CHECKIDENT reseed on tables that already contain data.

The values 1002, 1005, etc. are valid primary keys and can contain gaps after
records are deleted/recreated. Foreign keys depend on these values.

Use this diagnostic query only to inspect current identity counters:
*/

SELECT
    OBJECT_NAME(object_id) AS TableName,
    last_value AS CurrentIdentityValue
FROM sys.identity_columns
WHERE OBJECT_NAME(object_id) IN
(
    'users', 'booking', 'payment', 'login', 'vehicle', 'slot',
    'feedback', 'pricing', 'receipt', 'refunds', 'refund_receipt',
    'otp_verification'
)
ORDER BY TableName;
