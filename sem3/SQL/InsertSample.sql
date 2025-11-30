-- =============================================================================
-- Insert Sample Data
-- =============================================================================

USE OnlineRechargeDB;
GO

-- =============================================================================
-- 1. Insert 5 Users from 5 different countries
-- =============================================================================
INSERT INTO Users (MobileNumber, PasswordHash, FullName, Email, Address, WalletBalance, PostpaidMonthlyFee, Active)
VALUES 
    ('0912345678', '123456', 'Paul Muller', 'paulmuller@gmail.com', 'Berlin, Germany', 100.00, NULL, 1),
    
    -- User from USA (Washington D.C.)
    ('0923456789', '123456', 'John Smith', 'johnsmith@gmail.com', 'Washington D.C., USA', 150.50, 25.00, 1),
    
    -- User from Japan (Tokyo)
    ('0934567890', '123456', 'Yamamoto Hiroshi', 'yamamotohiroshi@gmail.com', 'Tokyo, Japan', 75.25, NULL, 1),
    
    -- User from France (Paris)
    ('0945678901', '123456', 'Marie Dubois', 'mariedubois@gmail.com', 'Paris, France', 200.75, 30.00, 1),
    
    -- User from Brazil (Bras�lia)
    ('0956789012', '123456' , 'Carlos Silva', 'carlossilva@gmail.com', 'Bras�lia, Brazil', 50.00, NULL, 1);
GO

-- =============================================================================
-- 2. Insert Admin User
-- =============================================================================
INSERT INTO AdminUsers (Username, PasswordHash, Email, MobileNumber)
VALUES 
    ('Admin', '123456', 'admin@gmail.com', '0987654321');
GO

-- =============================================================================
-- 3. Insert Recharge Plans for 3 operators
-- =============================================================================
INSERT INTO RechargePlans (PlanType, Amount, TalkTimeMinutes, DataMB, Details, Operator, IsActive)
VALUES 
    -- Vinaphone plans
    ('Data', 70000.00, 0, 2048, '2GB Data for 30 days', 'Vinaphone', 1),
    ('Prepaid', 50000.00, 60, 0, '60 minutes talk time', 'Vinaphone', 1),
    
    -- Mobiphone plans
    ('Data', 90000.00, 0, 4096, '4GB Data for 30 days', 'Mobiphone', 1),
    ('Prepaid', 100000.00, 120, 500, '120 minutes + 500MB data', 'Mobiphone', 1),
    
    -- Viettel plans
    ('Data', 120000.00, 0, 8192, '8GB Data for 30 days', 'Viettel', 1),
    ('Prepaid', 80000.00, 90, 1000, '90 minutes + 1GB data', 'Viettel', 1);
GO

-- =============================================================================
-- 4. Insert Transactions data for all 5 users
-- =============================================================================
INSERT INTO Transactions (MobileNumber, UserID, TransactionType, PlanID, Amount, TransactionDate, Status)
VALUES 
    -- Transactions for User 1 (0912345678)
    ('0912345678', 1, 'Recharge', 1, 70000.00, DATEADD(DAY, -10, GETDATE()), 'Success'),
    ('0912345678', 1, 'Recharge', 3, 90000.00, DATEADD(DAY, -5, GETDATE()), 'Success'),
    ('0912345678', 1, 'Wallet Top-up', NULL, 200000.00, DATEADD(DAY, -2, GETDATE()), 'Success'),
    
    -- Transactions for User 2 (0923456789)
    ('0923456789', 2, 'Recharge', 2, 50000.00, DATEADD(DAY, -15, GETDATE()), 'Success'),
    ('0923456789', 2, 'Postpaid Bill Payment', NULL, 25000.00, DATEADD(DAY, -8, GETDATE()), 'Success'),
    ('0923456789', 2, 'Wallet Top-up', NULL, 150000.00, DATEADD(DAY, -1, GETDATE()), 'Success'),
    
    -- Transactions for User 3 (0934567890)
    ('0934567890', 3, 'Recharge', 5, 120000.00, DATEADD(DAY, -12, GETDATE()), 'Success'),
    ('0934567890', 3, 'Recharge', 6, 80000.00, DATEADD(DAY, -3, GETDATE()), 'Pending'),
    ('0934567890', 3, 'Wallet Top-up', NULL, 100000.00, DATEADD(DAY, -20, GETDATE()), 'Success'),
    
    -- Transactions for User 4 (0945678901)
    ('0945678901', 4, 'Recharge', 4, 100000.00, DATEADD(DAY, -7, GETDATE()), 'Success'),
    ('0945678901', 4, 'Postpaid Bill Payment', NULL, 30000.00, DATEADD(DAY, -1, GETDATE()), 'Success'),
    ('0945678901', 4, 'Wallet Top-up', NULL, 250000.00, DATEADD(DAY, -5, GETDATE()), 'Failed'),
    
    -- Transactions for User 5 (0956789012)
    ('0956789012', 5, 'Recharge', 1, 70000.00, DATEADD(DAY, -4, GETDATE()), 'Success'),
    ('0956789012', 5, 'Recharge', 2, 50000.00, DATEADD(DAY, -18, GETDATE()), 'Success'),
    ('0956789012', 5, 'Wallet Top-up', NULL, 50000.00, DATEADD(DAY, -25, GETDATE()), 'Success');
GO

-- =============================================================================
-- 5. Insert TransactionScripts data (receipts for successful transactions)
-- =============================================================================
INSERT INTO TransactionScripts (TransactionID, ScriptContent)
VALUES 
    -- Receipts for User 1
    (1, 'Recharge successful for 0912345678. Plan: 2GB Data for 30 days. Amount: 70,000 VND. Transaction ID: TXN001. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -10, GETDATE()), 107)),
    (2, 'Recharge successful for 0912345678. Plan: 4GB Data for 30 days. Amount: 90,000 VND. Transaction ID: TXN002. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -5, GETDATE()), 107)),
    (3, 'Wallet top-up successful for 0912345678. Amount: 200,000 VND. New Balance: 300,000 VND. Transaction ID: TXN003. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -2, GETDATE()), 107)),
    
    -- Receipts for User 2
    (4, 'Recharge successful for 0923456789. Plan: 60 minutes talk time. Amount: 50,000 VND. Transaction ID: TXN004. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -15, GETDATE()), 107)),
    (5, 'Postpaid bill payment successful for 0923456789. Amount: 25,000 VND. Billing Cycle: ' + CONVERT(VARCHAR, DATEADD(MONTH, -1, GETDATE()), 107) + '. Transaction ID: TXN005. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -8, GETDATE()), 107)),
    (6, 'Wallet top-up successful for 0923456789. Amount: 150,000 VND. New Balance: 175,000 VND. Transaction ID: TXN006. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -1, GETDATE()), 107)),
    
    -- Receipts for User 3
    (7, 'Recharge successful for 0934567890. Plan: 8GB Data for 30 days. Amount: 120,000 VND. Transaction ID: TXN007. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -12, GETDATE()), 107)),
    (9, 'Wallet top-up successful for 0934567890. Amount: 100,000 VND. New Balance: 175,250 VND. Transaction ID: TXN009. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -20, GETDATE()), 107)),
    
    -- Receipts for User 4
    (10, 'Recharge successful for 0945678901. Plan: 120 minutes + 500MB data. Amount: 100,000 VND. Transaction ID: TXN010. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -7, GETDATE()), 107)),
    (11, 'Postpaid bill payment successful for 0945678901. Amount: 30,000 VND. Billing Cycle: ' + CONVERT(VARCHAR, DATEADD(MONTH, -1, GETDATE()), 107) + '. Transaction ID: TXN011. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -1, GETDATE()), 107)),
    
    -- Receipts for User 5
    (13, 'Recharge successful for 0956789012. Plan: 2GB Data for 30 days. Amount: 70,000 VND. Transaction ID: TXN013. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -4, GETDATE()), 107)),
    (14, 'Recharge successful for 0956789012. Plan: 60 minutes talk time. Amount: 50,000 VND. Transaction ID: TXN014. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -18, GETDATE()), 107)),
    (15, 'Wallet top-up successful for 0956789012. Amount: 50,000 VND. New Balance: 100,000 VND. Transaction ID: TXN015. Date: ' + CONVERT(VARCHAR, DATEADD(DAY, -25, GETDATE()), 107));
GO

-- =============================================================================
-- 6. Insert PaymentDetails data for transactions
-- =============================================================================
INSERT INTO PaymentDetails (TransactionID, PaymentMethod, ReferenceNumber)
VALUES 
    -- Payment methods for User 1
    (1, 'Wallet', 'WALLET_REF_001'),
    (2, 'Credit Card', 'CC_REF_002'),
    (3, 'Bank Transfer', 'BANK_REF_003'),
    
    -- Payment methods for User 2
    (4, 'Wallet', 'WALLET_REF_004'),
    (5, 'Wallet', 'WALLET_REF_005'),
    (6, 'Debit Card', 'DC_REF_006'),
    
    -- Payment methods for User 3
    (7, 'Wallet', 'WALLET_REF_007'),
    (8, 'Credit Card', 'CC_REF_008'),
    (9, 'Bank Transfer', 'BANK_REF_009'),
    
    -- Payment methods for User 4
    (10, 'Wallet', 'WALLET_REF_010'),
    (11, 'Wallet', 'WALLET_REF_011'),
    (12, 'Credit Card', 'CC_REF_012'),
    
    -- Payment methods for User 5
    (13, 'Wallet', 'WALLET_REF_013'),
    (14, 'Debit Card', 'DC_REF_014'),
    (15, 'Bank Transfer', 'BANK_REF_015');
GO

-- =============================================================================
-- 7. Insert PostpaidBills data for postpaid users
-- =============================================================================
INSERT INTO PostpaidBills (MobileNumber, BillingCycle, TotalAmount, PaymentDueDate, PaymentTransactionID, IsPaid)
VALUES 
    -- Postpaid bills for User 2 (0923456789)
    ('0923456789', DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE())-1, 1), 25000.00, DATEADD(DAY, 15, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE())-1, 1)), 5, 1),
    ('0923456789', DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1), 28000.00, DATEADD(DAY, 15, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)), -1, 0), -- Use -1 for unpaid bills
    
    -- Postpaid bills for User 4 (0945678901)
    ('0945678901', DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE())-1, 1), 30000.00, DATEADD(DAY, 15, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE())-1, 1)), 11, 1),
    ('0945678901', DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1), 32000.00, DATEADD(DAY, 15, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)), -2, 0); -- Use -2 for unpaid bills
GO

-- =============================================================================
-- 8. Insert Services
-- =============================================================================
INSERT INTO Services (ServiceName, ServiceDescription)
VALUES 
    ('Do Not Disturb', 'Block promotional calls and messages'),
    ('Caller Tunes', 'Customize your call waiting music');
GO

-- =============================================================================
-- 10. Create Trigger to automatically insert UserServiceSettings when new User is added
-- =============================================================================
CREATE OR ALTER TRIGGER trg_InsertUserServiceSettings
ON Users
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserID INT;
    DECLARE @ServiceID_DND INT;
    DECLARE @ServiceID_Tunes INT;
    
    -- Get Service IDs
    SELECT @ServiceID_DND = ServiceID FROM Services WHERE ServiceName = 'Do Not Disturb';
    SELECT @ServiceID_Tunes = ServiceID FROM Services WHERE ServiceName = 'Caller Tunes';
    
    -- Insert UserServiceSettings for each new user
    INSERT INTO UserServiceSettings (UserID, ServiceID, IsEnabled, SelectedTune, UpdatedDate)
    SELECT 
        i.UserID,
        s.ServiceID,
        0 AS IsEnabled, -- Default to disabled
        NULL AS SelectedTune, -- No tune selected by default
        GETDATE() AS UpdatedDate
    FROM inserted i
    CROSS JOIN Services s;
END;
GO


-- =============================================================================
-- Verify the data insertion
-- =============================================================================

-- Check Users
SELECT * FROM Users;
GO

-- Check AdminUsers
SELECT * FROM AdminUsers;
GO

-- Check RechargePlans
SELECT * FROM RechargePlans;
GO


-- Check Transactions
SELECT 'Transactions' AS TableName, COUNT(*) AS RecordCount FROM Transactions
UNION ALL
-- Check TransactionScripts
SELECT 'TransactionScripts', COUNT(*) FROM TransactionScripts
UNION ALL
-- Check PaymentDetails
SELECT 'PaymentDetails', COUNT(*) FROM PaymentDetails
UNION ALL
-- Check PostpaidBills
SELECT 'PostpaidBills', COUNT(*) FROM PostpaidBills;
GO

-- Display sample data from each table
SELECT '=== Transactions Sample ===' AS Info;
SELECT * FROM Transactions ORDER BY UserID, TransactionDate DESC;

SELECT '=== TransactionScripts Sample ===' AS Info;
SELECT * FROM TransactionScripts ORDER BY TransactionID;

SELECT '=== PaymentDetails Sample ===' AS Info;
SELECT * FROM PaymentDetails ORDER BY TransactionID;

SELECT '=== PostpaidBills Sample ===' AS Info;
SELECT * FROM PostpaidBills ORDER BY BillingCycle DESC, MobileNumber;
GO

-- Check Services
SELECT * FROM Services;
GO

-- Check UserServiceSettings (should have entries for all users after trigger execution)
SELECT * FROM UserServiceSettings;
GO

INSERT INTO SystemSettings (SettingKey, SettingValue, Description)
VALUES 
('Contact_Address', N'123 Recharge Street<br>Digital District<br>Ho Chi Minh City, Vietnam', 'Office Address'),
('Contact_PhoneMain', N'+84 28 3844 8888', 'Main Phone Number'),
('Contact_PhoneSupport', N'+84 28 3844 9999', 'Support Phone Number'),
('Contact_Email1', N'support@rechargesystem.vn', 'Support Email 1'),
('Contact_Email2', N'info@rechargesystem.vn', 'Support Email 2'),
('Contact_HoursWeekdays', N'8:00 AM - 10:00 PM', 'Working Hours (Mon-Fri)'),
('Contact_HoursWeekend', N'9:00 AM - 8:00 PM', 'Working Hours (Sat-Sun)');
GO