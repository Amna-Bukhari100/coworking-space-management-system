-- -- USE [CoWorkManager]; 
-- -- GO

-- -- -- Clear old data to prevent duplicates
-- -- DELETE FROM Workspaces;

-- -- -- Insert professional workspaces matching your Workspace model
-- -- INSERT INTO Workspaces (Name, Type, PricePerHour, IsAvailable) 
-- -- VALUES 
-- -- -- Type: Desk
-- -- ('Lumina Hot Desk A1', 'Desk', 5.00, 1),
-- -- ('Lumina Hot Desk A2', 'Desk', 5.00, 1),
-- -- ('Window-Side Dedicated Desk', 'Desk', 8.50, 1),

-- -- -- Type: Booth
-- -- ('Soundproof Call Pod 01', 'Booth', 12.00, 1),
-- -- ('Privacy Focus Booth', 'Booth', 10.00, 1),

-- -- -- Type: Meeting Room
-- -- ('Synergy Meeting Room (6-Pax)', 'Meeting Room', 25.00, 1),
-- -- ('The Nexus Conference Hall', 'Meeting Room', 65.00, 1),

-- -- -- Type: Office
-- -- ('Executive Suite: Zenith', 'Office', 50.00, 1),
-- -- ('Collaborative Team Suite', 'Office', 40.00, 1);

-- -- -- Verify
-- -- SELECT WorkspaceId, Name, Type, PricePerHour, IsAvailable FROM Workspaces;
-- USE [CoWorkManager];
-- GO

-- -- This updates the column to match your new C# nullable model
-- ALTER TABLE Bookings ALTER COLUMN VisitorId INT NULL;
-- GO
USE [CoWorkManager];
GO

/* 1. DROP INDEX AND COLUMN FROM BOOKINGS
   We must drop the index 'IX_Bookings_VisitorId' before we can drop the column.
*/
-- IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Bookings_VisitorId' AND object_id = OBJECT_ID(N'Bookings'))
-- BEGIN
--     DROP INDEX [IX_Bookings_VisitorId] ON [Bookings];
-- END
-- GO

-- IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'VisitorId' AND Object_ID = Object_ID(N'Bookings'))
-- BEGIN
--     -- If there is a foreign key constraint, we drop it too
--     DECLARE @ConstraintName nvarchar(200)
--     SELECT @ConstraintName = Name FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Bookings') AND referenced_object_id = OBJECT_ID('Visitors')
--     IF @ConstraintName IS NOT NULL
--         EXEC('ALTER TABLE Bookings DROP CONSTRAINT ' + @ConstraintName)

--     ALTER TABLE Bookings DROP COLUMN VisitorId;
-- END
-- GO

-- /* 2. UPDATE VISITORS TABLE
--    Establish the 1-to-Many link: Every visitor now points to a Booking.
-- */
-- IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'BookingId' AND Object_ID = Object_ID(N'Visitors'))
-- BEGIN
--     ALTER TABLE Visitors ADD BookingId INT NOT NULL;
    
--     ALTER TABLE Visitors 
--     ADD CONSTRAINT FK_Visitors_Bookings FOREIGN KEY (BookingId) 
--     REFERENCES Bookings(BookingId) ON DELETE CASCADE;
-- END
-- GO

-- /* 3. REFRESH WORKSPACES
--    Clean data for the new Time-Based system.
-- */
-- DELETE FROM Visitors;
-- DELETE FROM Bookings;
-- DELETE FROM Workspaces;

-- INSERT INTO Workspaces (Name, Type, PricePerHour, IsAvailable) 
-- VALUES 
-- ('Lumina Hot Desk A1', 'Desk', 5.00, 1),
-- ('Lumina Hot Desk A2', 'Desk', 5.00, 1),
-- ('Privacy Pod 101', 'Booth', 12.00, 1),
-- ('Synergy Meeting Room', 'Meeting Room', 25.00, 1),
-- ('The Nexus Hall', 'Meeting Room', 65.00, 1),
-- ('Executive Suite', 'Office', 50.00, 1);
-- GO
USE [CoWorkManager];
GO