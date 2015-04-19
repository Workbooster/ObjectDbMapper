IF EXISTS (SELECT * FROM sys.objects WHERE name = N'Addresses' AND type = 'U') 
BEGIN
	DROP TABLE [dbo].[Addresses]
END

IF EXISTS (SELECT * FROM sys.objects WHERE name = N'People' AND type = 'U') 
BEGIN
	DROP TABLE [dbo].[People]
END

CREATE TABLE [dbo].[People] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50) NOT NULL,
    [IsMarried]    TINYINT       NOT NULL,
    [DateOfBirth]  DATE          NOT NULL,
    [PlaceOfBirth] NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Addresses] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [IdPerson]       INT           NOT NULL,
    [Street]         NVARCHAR (50) NOT NULL,
    [ZipCode]        NVARCHAR (20) NOT NULL,
    [City]           NVARCHAR (50) NOT NULL,
    [IsPrimary]      TINYINT       NOT NULL,
    [DateOfCreation] DATETIME      DEFAULT (((1900)-(1))-(1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Addresses_ToPeople] FOREIGN KEY ([IdPerson]) REFERENCES [dbo].[People] ([Id])
);

SET IDENTITY_INSERT [dbo].[People] ON
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (2, N'Mike', 0, N'1985-06-13', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (3, N'Steve', 0, N'1978-02-03', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (4, N'Meg', 1, N'1965-03-09', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (5, N'Melanie', 0, N'1988-11-27', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (6, N'Becky', 0, N'1972-08-21', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (7, N'Larry', 0, N'1969-01-26', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (8, N'Mike', 1, N'1953-09-23', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (9, N'mike', 0, N'1972-02-22', NULL)
INSERT INTO [dbo].[People] ([Id], [Name], [IsMarried], [DateOfBirth], [PlaceOfBirth]) VALUES (10, N'Samuel', 1, N'1958-11-27', NULL)
SET IDENTITY_INSERT [dbo].[People] OFF

SET IDENTITY_INSERT [dbo].[Addresses] ON
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (1, 2, N'Schaffhauserstr. 119', N'8005', N'Zurich', 1, N'2014-01-15 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (2, 2, N'Kempttalstr. 70', N'8640', N'Fehraltorf', 0, N'2014-03-27 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (3, 3, N'Baslerstrasse. 320', N'8004', N'Zurich', 1, N'2014-05-22 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (4, 4, N'Wehntalerstr. 89', N'8157', N'Dielsdorf', 1, N'2014-03-11 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (5, 5, N'Grafschaftstr. 52', N'8172', N'Niederglatt', 0, N'2014-11-22 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (6, 5, N'Grafschaftstr. 54', N'8172', N'Niederglatt', 1, N'2014-06-21 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (7, 5, N'Via A. Ciseri 13', N'6600', N'Locarno', 0, N'2014-08-01 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (8, 6, N'Seetalstrasse 6', N'5630', N'Muri', 1, N'2014-07-09 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (9, 7, N'Albisriederstr 167', N'8047', N'Zurich', 0, N'2014-12-17 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (10, 7, N'Jurastrasse 22', N'4901', N'Langenthal', 1, N'2014-04-01 00:00:00')
INSERT INTO [dbo].[Addresses] ([Id], [IdPerson], [Street], [ZipCode], [City], [IsPrimary], [DateOfCreation]) VALUES (11, 7, N'Casa Calanca', N'6633', N'Lavertezzo', 0, N'2014-09-15 00:00:00')
SET IDENTITY_INSERT [dbo].[Addresses] OFF