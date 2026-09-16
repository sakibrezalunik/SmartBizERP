/* =====================================================
   SmartBizERP - Phase 6 Milestone
   Security Tables: Users, Roles, UserRoles, AuditLogs
   ===================================================== */

USE SmartBizERP;
GO

-- ============================
-- Roles
-- ============================
CREATE TABLE Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200)
);
GO

-- ============================
-- Users
-- PasswordHash stores a SHA-256 hash, never the plain password.
-- ============================
CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- ============================
-- UserRoles (many-to-many: a user can hold multiple roles)
-- ============================
CREATE TABLE UserRoles
(
    UserId INT NOT NULL,
    RoleId INT NOT NULL,

    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),

    CONSTRAINT FK_UserRoles_User
        FOREIGN KEY (UserId) REFERENCES Users(UserId),

    CONSTRAINT FK_UserRoles_Role
        FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

-- ============================
-- AuditLogs
-- Who did what, when. Not wired into every action yet --
-- that's Phase 8 polish -- but the table exists now so
-- the login/logout events have somewhere to be recorded.
-- ============================
CREATE TABLE AuditLogs
(
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    EntityName NVARCHAR(100),
    EntityId INT,
    Details NVARCHAR(500),
    ActionDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_AuditLogs_User
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- ============================
-- Seed: one Admin role, one admin login
-- Username: admin
-- Password: Admin@123
-- (change this after your first login)
-- ============================
INSERT INTO Roles (RoleName, Description) VALUES
('Admin', 'Full system access'),
('Staff', 'Standard operational access');
GO

INSERT INTO Users (Username, PasswordHash, FullName, Email, IsActive)
VALUES (
    'admin',
    CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', N'Admin@123'), 2),
    'System Administrator',
    'admin@smartbizerp.local',
    1
);
GO

INSERT INTO UserRoles (UserId, RoleId)
SELECT u.UserId, r.RoleId
FROM Users u, Roles r
WHERE u.Username = 'admin' AND r.RoleName = 'Admin';
GO

/* =====================================================
   Verify
   ===================================================== */
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

SELECT * FROM Users;
SELECT * FROM Roles;
SELECT * FROM UserRoles;
GO
