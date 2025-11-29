USE master
GO

CREATE DATABASE OnlineRechargeDB;
GO

USE OnlineRechargeDB;
GO

-- =============================================================================
-- Table Definitions
-- =============================================================================

-- 1. Users Table - Stores registered user information
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber NVARCHAR(10) NOT NULL UNIQUE CHECK (LEN(MobileNumber) = 10),
    PasswordHash NVARCHAR(128) NOT NULL,
    FullName NVARCHAR(100),
    Email VARCHAR(100),
    Address NVARCHAR(255),
    RegistrationDate DATETIME DEFAULT GETDATE(),
    WalletBalance DECIMAL(18,2) DEFAULT 0,
    PostpaidMonthlyFee DECIMAL(10,2) NULL
);
GO

-- 2. AdminUsers Table - Stores administrator account information
CREATE TABLE AdminUsers (
    AdminID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(128) NOT NULL,
    Email VARCHAR(100),
    MobileNumber VARCHAR(10)
);
GO

-- 3. RechargePlans Table - Stores available recharge packages and plans
CREATE TABLE RechargePlans (
    PlanID INT PRIMARY KEY IDENTITY(1,1),
    PlanType VARCHAR(50) NOT NULL, --'Data' or 'Prepaid'
    Amount DECIMAL(10,2) NOT NULL,
    TalkTimeMinutes INT,
    DataMB INT,
    Details NVARCHAR(255),
    Operator NVARCHAR(50) NOT NULL DEFAULT 'Unknown',
    IsActive BIT DEFAULT 1
);
GO

-- 4. Transactions Table - Stores all recharge and payment transactions
CREATE TABLE Transactions (
    TransactionID INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber VARCHAR(10) NOT NULL CHECK (LEN(MobileNumber) = 10),
    UserID INT NULL,
    TransactionType VARCHAR(50) NOT NULL,
    PlanID INT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- 5. TransactionScripts Table - Stores transaction receipts and scripts
CREATE TABLE TransactionScripts (
    ScriptID INT PRIMARY KEY IDENTITY(1,1),
    TransactionID INT NOT NULL UNIQUE,
    ScriptContent TEXT NOT NULL,
    FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionID)
);
GO

-- 6. PaymentDetails Table - Stores payment method information for transactions
CREATE TABLE PaymentDetails (
    PaymentDetailID INT PRIMARY KEY IDENTITY(1,1),
    TransactionID INT NOT NULL UNIQUE,
    PaymentMethod VARCHAR(50) NOT NULL,
    ReferenceNumber VARCHAR(100) UNIQUE,
    FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionID)
);
GO

-- 7. PostpaidBills Table - Stores postpaid billing information
CREATE TABLE PostpaidBills (
    BillID INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber VARCHAR(10) NOT NULL CHECK (LEN(MobileNumber) = 10),
    BillingCycle DATE NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    PaymentDueDate DATE,
    PaymentTransactionID INT NULL UNIQUE,
    IsPaid BIT DEFAULT 0,
    FOREIGN KEY (PaymentTransactionID) REFERENCES Transactions(TransactionID)
);
GO

-- 8. Services Table - Stores available services such as Do Not Disturb, Caller Tunes
CREATE TABLE Services (
    ServiceID INT PRIMARY KEY IDENTITY(1,1),
    ServiceName NVARCHAR(100) NOT NULL UNIQUE,
    ServiceDescription NVARCHAR(255)
);
GO

-- 9. UserServices Table - Stores services that have been activated for users
CREATE TABLE UserServices (
    UserServiceID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ServiceID INT NOT NULL,
    ActivationDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID),
    UNIQUE (UserID, ServiceID)
);
GO

-- 10. UserServiceSettings Table - Stores user-specific service configuration settings
CREATE TABLE UserServiceSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    ServiceID INT NOT NULL,
    IsEnabled BIT DEFAULT 0,
    SelectedTune VARCHAR(100) NULL,
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID),
    UNIQUE (UserID, ServiceID)
);
GO

-- 11. Feedback Table - Stores user feedback and ratings
CREATE TABLE Feedback (
    FeedbackID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NULL,
    Name VARCHAR(100),
    Email VARCHAR(100),
    FeedbackText TEXT NOT NULL,
    SubmitDate DATETIME DEFAULT GETDATE(),
    Rating INT,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- 12. SiteContent Table - Stores static content for pages like About Us and Site Map
CREATE TABLE SiteContent (
    ContentID INT PRIMARY KEY IDENTITY(1,1),
    PageName VARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(100),
    ContentText NVARCHAR(MAX)
);
GO

-- 13. ContactPoints Table - Stores contact information for different contact types
CREATE TABLE ContactPoints (
    ContactID INT PRIMARY KEY IDENTITY(1,1),
    ContactType VARCHAR(50) NOT NULL UNIQUE,
    Details NVARCHAR(255) NOT NULL,
    Description NVARCHAR(255)
);
GO