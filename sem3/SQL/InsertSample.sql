-- =============================================================================
-- Insert Sample Data
-- =============================================================================

USE OnlineRechargeDB;
GO

-- =============================================================================
-- 1. Insert 5 Users from 5 different countries
-- =============================================================================
INSERT INTO Users (MobileNumber, PasswordHash, FullName, Email, Address) VALUES
-- American user
('0833456789', '123456', 'Johnathan Miller', 'johnathan.miller@gmail.com', 'Washington D.C., United States'),
-- Russian user
('0963456789', '123456', 'Alexei Ivanov', 'alexei.ivanov@gmail.com', 'Moscow, Russia'),
-- Japanese user
('0934567890', '123456', 'Hiroshi Tanaka', 'hiroshi.tanaka@gmail.com', 'Tokyo, Japan'),
-- French user
('0945678901', '123456', 'Marie Dubois', 'marie.dubois@gmail.com', 'Paris, France'),
-- Chinese user
('0916789012', '123456', 'Li Wei', 'li.wei@gmail.com', 'Beijing, China');
GO

-- =============================================================================
-- 2. Insert Admin User
-- =============================================================================
INSERT INTO AdminUsers (Username, PasswordHash, Email, MobileNumber) VALUES
('Admin', '123456', 'admin@onlinerecharge.com', '0987654321');
GO

-- =============================================================================
-- 3. Insert Recharge Plans for 3 operators
-- =============================================================================
INSERT INTO RechargePlans (PlanName, PlanType, Amount, TalkTimeMinutes, DataMB, Details, Operator, IsActive)
VALUES 
    -- Vinaphone plans
    ('Vina1' ,'Data', 70000.00, 0, 2048, '2GB Data for 30 days', 'Vinaphone', 1),
    ('Vina2' ,'Prepaid', 50000.00, 60, 0, '60 minutes talk time', 'Vinaphone', 1),
    
    -- Mobiphone plans
    ('Mobi1' ,'Data', 90000.00, 0, 4096, '4GB Data for 30 days', 'Mobiphone', 1),
    ('Mobi2' ,'Prepaid', 100000.00, 120, 500, '120 minutes + 500MB data', 'Mobiphone', 1),
    
    -- Viettel plans
    ('Viet1' ,'Data', 120000.00, 0, 8192, '8GB Data for 30 days', 'Viettel', 1),
    ('Viet2' ,'Prepaid', 80000.00, 90, 1000, '90 minutes + 1GB data', 'Viettel', 1);
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

DECLARE @ConstraintName NVARCHAR(200)

SELECT @ConstraintName = name 
FROM sys.key_constraints 
WHERE parent_object_id = OBJECT_ID('PostpaidBills') 
AND type = 'UQ'

IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE PostpaidBills DROP CONSTRAINT ' + @ConstraintName)
    PRINT 'Đã xóa constraint: ' + @ConstraintName
END
ELSE
BEGIN
    PRINT 'Không tìm thấy constraint UNIQUE trên PostpaidBills'
END
GO

-- =============================================================================
-- 7. Insert PostpaidBills data for postpaid users
-- =============================================================================
INSERT INTO PostpaidBills (MobileNumber, BillingCycle, TotalAmount, PaymentDueDate, IsPaid) VALUES
-- User 1: Johnathan Miller (0833456789)
-- Bill 1: Paid bill for October cycle
('0833456789', '2025-10-01', 285750.00, '2025-10-31', 1),
-- Bill 2: Unpaid bill for November cycle
('0833456789', '2025-11-01', 312450.50, '2025-11-30', 0),

-- User 2: Alexei Ivanov (0963456789)
-- Bill 1: Paid bill for September cycle
('0963456789', '2025-09-01', 267890.75, '2025-09-30', 1),
-- Bill 2: Unpaid bill for November cycle
('0963456789', '2025-11-01', 298760.00, '2025-11-30', 0),

-- User 3: Hiroshi Tanaka (0934567890)
-- Bill 1: Paid bill for October cycle
('0934567890', '2025-10-01', 234560.25, '2025-10-31', 1),
-- Bill 2: Unpaid bill for November cycle
('0934567890', '2025-11-01', 276890.00, '2025-11-30', 0),

-- User 4: Marie Dubois (0945678901)
-- Bill 1: Paid bill for September cycle
('0945678901', '2025-09-01', 319875.50, '2025-09-30', 1),
-- Bill 2: Unpaid bill for November cycle
('0945678901', '2025-11-01', 342150.75, '2025-11-30', 0),

-- User 5: Li Wei (0916789012)
-- Bill 1: Paid bill for October cycle
('0916789012', '2025-10-01', 289450.00, '2025-10-31', 1),
-- Bill 2: Unpaid bill for November cycle
('0916789012', '2025-11-01', 315670.25, '2025-11-30', 0);
GO

PRINT 'Data insertion completed successfully for all specified tables.';

-- =============================================================================
-- 8. Insert Services
-- =============================================================================
INSERT INTO Services (ServiceName, ServiceDescription)
VALUES 
    ('Do Not Disturb', 'Block promotional calls and messages'),
    ('Caller Tunes', 'Customize your call waiting music');
GO

-- Insert sample feedback records
INSERT INTO Feedback (UserID, Name, Email, FeedbackText, Rating) VALUES
(1, 'Johnathan Miller', 'johnathan.miller@gmail.com', 'The recharge process was very smooth and fast. Highly recommended service.', 5),
(2, 'Alexei Ivanov', 'alexei.ivanov@gmail.com', 'Good service with reasonable plans. Customer support was helpful.', 4),
(3, 'Hiroshi Tanaka', 'hiroshi.tanaka@gmail.com', 'Very convenient platform for mobile recharges. Will use again.', 5);
GO

-- Insert sample FAQ entries
INSERT INTO FAQs (Question, Answer, OrderIndex) VALUES
('How long does it take for a recharge to be processed?', 'Recharges are typically processed instantly. In rare cases where instant recharge is not possible, it will be credited within 2 hours.', 1),
('What payment methods are accepted?', 'We accept payments through Bank Cards, Mobile Wallets, Internet Banking, and Bank Transfers from all major banks.', 2),
('Can I recharge a number that is not registered in my name?', 'Yes, you can recharge any valid mobile number regardless of whether it is registered in your name.', 3),
('What happens if there is a payment failure?', 'If a payment fails, the amount will be automatically refunded to the original payment source within 5-7 business days.', 4);
GO

-- Insert system settings for contact information and other configuration values
INSERT INTO SystemSettings (SettingKey, SettingValue, Description) VALUES
('CustomerSupportPhone', '18001090', 'Primary customer support contact number'),
('CustomerSupportEmail', 'support@onlinerecharge.com', 'Customer support email address'),
('CompanyName', 'Online Recharge Service', 'Company name displayed on the platform'),
('SupportHours', '24/7', 'Customer support availability'),
('RefundPolicy', 'Full refund within 2 hours for failed transactions', 'Refund policy description');
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

