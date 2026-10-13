-- ParkMate SQL Server schema reference generated from the initial EF Core migration.
-- SCHEMA ONLY: no user, booking, payment, OTP, or other application records are included.
-- Use against a NEW, EMPTY ParkMateDb. Alternatively, apply the EF migration with Update-Database; do not do both.

CREATE TABLE dbo.pricing (
    PriceId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_pricing PRIMARY KEY,
    VType nvarchar(450) NOT NULL,
    PricePerHour int NOT NULL,
    PricePerDay int NOT NULL,
    UpdatedAt datetime2 NOT NULL
);

CREATE TABLE dbo.role (
    RoleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_role PRIMARY KEY,
    RoleName nvarchar(max) NOT NULL
);

CREATE TABLE dbo.slot (
    SlotId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_slot PRIMARY KEY,
    SlotNum nvarchar(450) NOT NULL,
    SlotType nvarchar(max) NOT NULL,
    SlotStatus nvarchar(max) NOT NULL
);

CREATE TABLE dbo.users (
    UId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_users PRIMARY KEY,
    Name nvarchar(max) NOT NULL,
    Email nvarchar(450) NOT NULL,
    Contact nvarchar(450) NULL,
    Password nvarchar(max) NOT NULL,
    RoleId int NOT NULL,
    IsVerified bit NOT NULL,
    CreatedAt datetime2 NOT NULL,
    CONSTRAINT FK_users_role_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.role(RoleId) ON DELETE NO ACTION
);

CREATE TABLE dbo.login (
    LoginId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_login PRIMARY KEY,
    UId int NOT NULL,
    LoginTime datetime2 NOT NULL,
    LoginType nvarchar(max) NOT NULL,
    LoginStatus nvarchar(max) NOT NULL,
    CONSTRAINT FK_login_users_UId FOREIGN KEY (UId) REFERENCES dbo.users(UId) ON DELETE NO ACTION
);

CREATE TABLE dbo.otp_verification (
    OtpId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_otp_verification PRIMARY KEY,
    UId int NOT NULL,
    OtpCode nvarchar(max) NOT NULL,
    Purpose nvarchar(max) NOT NULL,
    ExpiresAt datetime2 NOT NULL,
    IsUsed bit NOT NULL,
    AttemptCount int NOT NULL,
    BlockedUntil datetime2 NULL,
    CreatedAt datetime2 NOT NULL,
    CONSTRAINT FK_otp_verification_users_UId FOREIGN KEY (UId) REFERENCES dbo.users(UId) ON DELETE NO ACTION
);

CREATE TABLE dbo.vehicle (
    VId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_vehicle PRIMARY KEY,
    UId int NOT NULL,
    VNum nvarchar(450) NOT NULL,
    VType nvarchar(max) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    CONSTRAINT FK_vehicle_users_UId FOREIGN KEY (UId) REFERENCES dbo.users(UId) ON DELETE NO ACTION
);

CREATE TABLE dbo.booking (
    BId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_booking PRIMARY KEY,
    UId int NOT NULL,
    VId int NOT NULL,
    VType nvarchar(max) NOT NULL,
    SlotId int NOT NULL,
    StartTime datetime2 NOT NULL,
    EndTime datetime2 NOT NULL,
    TotalAmount decimal(18,2) NOT NULL,
    PaymentStatus nvarchar(max) NOT NULL,
    BStatus nvarchar(max) NOT NULL,
    BookingDate datetime2 NOT NULL,
    CancelledAt datetime2 NULL,
    CancelReason nvarchar(max) NULL,
    RefundAmount decimal(18,2) NULL,
    CONSTRAINT FK_booking_slot_SlotId FOREIGN KEY (SlotId) REFERENCES dbo.slot(SlotId) ON DELETE NO ACTION,
    CONSTRAINT FK_booking_users_UId FOREIGN KEY (UId) REFERENCES dbo.users(UId) ON DELETE NO ACTION,
    CONSTRAINT FK_booking_vehicle_VId FOREIGN KEY (VId) REFERENCES dbo.vehicle(VId) ON DELETE NO ACTION
);

CREATE TABLE dbo.feedback (
    FId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_feedback PRIMARY KEY,
    BId int NOT NULL,
    Rating tinyint NOT NULL,
    Comment nvarchar(max) NULL,
    CreatedAt datetime2 NOT NULL,
    CONSTRAINT FK_feedback_booking_BId FOREIGN KEY (BId) REFERENCES dbo.booking(BId) ON DELETE NO ACTION
);

CREATE TABLE dbo.payment (
    PId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_payment PRIMARY KEY,
    BId int NOT NULL,
    PaidAmount decimal(18,2) NOT NULL,
    PaymentType nvarchar(max) NOT NULL,
    Mode nvarchar(max) NOT NULL,
    PayStatus nvarchar(max) NOT NULL,
    PayDate datetime2 NOT NULL,
    CONSTRAINT FK_payment_booking_BId FOREIGN KEY (BId) REFERENCES dbo.booking(BId) ON DELETE NO ACTION
);

CREATE TABLE dbo.refunds (
    RefundId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_refunds PRIMARY KEY,
    BId int NOT NULL,
    UId int NOT NULL,
    Amount decimal(18,2) NOT NULL,
    RefundStatus nvarchar(max) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    CONSTRAINT FK_refunds_booking_BId FOREIGN KEY (BId) REFERENCES dbo.booking(BId) ON DELETE NO ACTION,
    CONSTRAINT FK_refunds_users_UId FOREIGN KEY (UId) REFERENCES dbo.users(UId) ON DELETE NO ACTION
);

CREATE TABLE dbo.receipt (
    RecId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_receipt PRIMARY KEY,
    PId int NOT NULL,
    RecNo nvarchar(450) NOT NULL,
    GeneratedAt datetime2 NOT NULL,
    CONSTRAINT FK_receipt_payment_PId FOREIGN KEY (PId) REFERENCES dbo.payment(PId) ON DELETE NO ACTION
);

CREATE TABLE dbo.refund_receipt (
    RefundRecId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_refund_receipt PRIMARY KEY,
    RefundId int NOT NULL,
    RecNo nvarchar(450) NOT NULL,
    GeneratedAt datetime2 NOT NULL,
    CONSTRAINT FK_refund_receipt_refunds_RefundId FOREIGN KEY (RefundId) REFERENCES dbo.refunds(RefundId) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IX_pricing_VType ON dbo.pricing(VType);
CREATE UNIQUE INDEX IX_slot_SlotNum ON dbo.slot(SlotNum);
CREATE UNIQUE INDEX IX_users_Email ON dbo.users(Email);
CREATE UNIQUE INDEX IX_users_Contact ON dbo.users(Contact) WHERE Contact IS NOT NULL;
CREATE INDEX IX_users_RoleId ON dbo.users(RoleId);
CREATE INDEX IX_login_UId ON dbo.login(UId);
CREATE INDEX IX_otp_verification_UId ON dbo.otp_verification(UId);
CREATE INDEX IX_vehicle_UId ON dbo.vehicle(UId);
CREATE UNIQUE INDEX IX_vehicle_VNum ON dbo.vehicle(VNum);
CREATE INDEX IX_booking_SlotId ON dbo.booking(SlotId);
CREATE INDEX IX_booking_UId ON dbo.booking(UId);
CREATE INDEX IX_booking_VId ON dbo.booking(VId);
CREATE INDEX IX_feedback_BId ON dbo.feedback(BId);
CREATE INDEX IX_payment_BId ON dbo.payment(BId);
CREATE UNIQUE INDEX IX_refunds_BId ON dbo.refunds(BId);
CREATE INDEX IX_refunds_UId ON dbo.refunds(UId);
CREATE INDEX IX_receipt_PId ON dbo.receipt(PId);
CREATE UNIQUE INDEX IX_receipt_RecNo ON dbo.receipt(RecNo);
CREATE UNIQUE INDEX IX_refund_receipt_RecNo ON dbo.refund_receipt(RecNo);
CREATE UNIQUE INDEX IX_refund_receipt_RefundId ON dbo.refund_receipt(RefundId);
