USE [BillMaker]
GO

/****** Object:  Table [dbo].[CompanySettings]    Script Date: 31-01-2021 11:08:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CompanySettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


SET IDENTITY_INSERT [dbo].[CompanySettings] ON
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (6, N'CompanyName', N'<Compnay Name>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (7, N'CompanyPhone', N'<Compnay Phone>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (8, N'CompanyGSTINNo', N'<Compnay GSTIN No>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (9, N'CompanyEmailId', N'<Compnay Email Id>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (10, N'CompanyTANNo', N'<Compnay TAN no>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (11, N'CompanyAccountNumber', N'<Compnay Account Number>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (12, N'ComapnyIFSCCode', N'<Compnay IFSC Cod>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (13, N'CompanyRGST', N'<Compnay RTGS NO>')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (14, N'IsShowBankDetails', N'1')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (15, N'DefaultPrinter', N'')
INSERT INTO [dbo].[CompanySettings] ([Id], [Name], [Value]) VALUES (16, N'CompanyAddress', N'')
SET IDENTITY_INSERT [dbo].[CompanySettings] OFF


/****** Object:  Table [dbo].[Person]    Script Date: 30-01-2021 11:48:19 ******/

CREATE TABLE [dbo].[Person](
	[PersonId] [int] IDENTITY(1,1) NOT NULL,
	[PersonName] [nvarchar](max) NOT NULL,
	[Phone] [nvarchar](13) NULL,
	[Email] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[AddedDate] [datetime] NOT NULL,
	[IsVendor] [bit] NOT NULL,
	[IsCustomer] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Person] ADD  CONSTRAINT [DF_Person_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO



/****** Object:  Table [dbo].[Products]    Script Date: 30-01-2021 11:48:45 ******/

CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](max) NOT NULL,
	[HSNCode] [varchar](8) NOT NULL,
	[Cgst] [decimal](5, 2) NOT NULL,
	[Sgst] [decimal](5, 2) NOT NULL,
	[description] [varchar](max) NULL,
	[IsRawMaterial] [bit] NOT NULL,
	[IsProduct] [bit] NOT NULL,
	[IsUnitsConnected] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_HSNCode]  DEFAULT (('0')) FOR [HSNCode]
GO



/****** Object:  Table [dbo].[ProductUnits]    Script Date: 27-02-2021 12:33:10 ******/


CREATE TABLE [dbo].[ProductUnits](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[UnitName] [varchar](25) NOT NULL,
	[Conversion] [int] NOT NULL,
	[Stock] [decimal](12, 2) NOT NULL,
	[IsBasicUnit] [bit] NOT NULL,
	[IsPurchaseUnit] [bit] NOT NULL,
	[UnitBuyPrice] [decimal](10, 2) NOT NULL,
	[UnitSellPrice] [decimal](10, 2) NOT NULL,
	[IsActive]      [bit] NOT NULL,
 CONSTRAINT [PK_ProductUnits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_Conversion]  DEFAULT ((1)) FOR [Conversion]
GO

ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_Stock]  DEFAULT ((0)) FOR [Stock]
GO

ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_UnitBuyPrice]  DEFAULT ((0)) FOR [UnitBuyPrice]
GO

ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_UnitSellPrice]  DEFAULT ((0)) FOR [UnitSellPrice]
GO

ALTER TABLE [dbo].[ProductUnits] ADD  CONSTRAINT [DF_ProductUnits_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[ProductUnits]  WITH CHECK ADD  CONSTRAINT [FK_ProductUnit_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[ProductUnits] CHECK CONSTRAINT [FK_ProductUnit_Products]
GO


/****** Object:  Table [dbo].[StockLog]    Script Date: 18-03-2021 11:49:22 ******/
CREATE TABLE [dbo].[StockLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductUnitId] [int] NOT NULL,
	[AddedValue] [decimal](12, 2) NOT NULL,
	[AddedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[StockLog]  WITH CHECK ADD  CONSTRAINT [FK_StockLog_ProductUnits] FOREIGN KEY([ProductUnitId])
REFERENCES [dbo].[ProductUnits] ([Id])
GO

ALTER TABLE [dbo].[StockLog] CHECK CONSTRAINT [FK_StockLog_ProductUnits]
GO


/****** Object:  Table [dbo].[Sale]    Script Date: 30-01-2021 11:49:22 ******/

CREATE TABLE [dbo].[Sale](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[PersonName] [nvarchar](max) NULL,
	[SellType] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Sale]  WITH CHECK ADD  CONSTRAINT [FK_Sale_Person] FOREIGN KEY([PersonId])
REFERENCES [dbo].[Person] ([PersonId])
GO

ALTER TABLE [dbo].[Sale] CHECK CONSTRAINT [FK_Sale_Person]
GO


/****** Object:  Table [dbo].[order_details]    Script Date:  27-02-2021 12:34:36 ******/

CREATE TABLE [dbo].[order_details](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[SaleId] [int] NOT NULL,
	[UnitId] [int] NOT NULL,
	[Quantity] [decimal](10, 2) NOT NULL,
	[TotalPrice] [decimal](15, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[order_details]  WITH CHECK ADD  CONSTRAINT [FK_order_details_product] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO

ALTER TABLE [dbo].[order_details] CHECK CONSTRAINT [FK_order_details_product]
GO

ALTER TABLE [dbo].[order_details]  WITH CHECK ADD  CONSTRAINT [FK_order_details_ProductUnits] FOREIGN KEY([UnitId])
REFERENCES [dbo].[ProductUnits] ([Id])
GO

ALTER TABLE [dbo].[order_details] CHECK CONSTRAINT [FK_order_details_ProductUnits]
GO

ALTER TABLE [dbo].[order_details]  WITH CHECK ADD  CONSTRAINT [FK_order_details_sale] FOREIGN KEY([SaleId])
REFERENCES [dbo].[Sale] ([Id])
GO

ALTER TABLE [dbo].[order_details] CHECK CONSTRAINT [FK_order_details_sale]
GO



/****** Object:  Table [dbo].[Transaction]    Script Date: 30-01-2021 11:58:14 ******/

CREATE TABLE [dbo].[Transaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PaymentType] [int] NOT NULL,
	[Amount] [decimal](15, 2) NOT NULL,
	[SaleId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Transaction]  WITH CHECK ADD  CONSTRAINT [FK_Transaction_Sale] FOREIGN KEY([SaleId])
REFERENCES [dbo].[Sale] ([Id])
GO

ALTER TABLE [dbo].[Transaction] CHECK CONSTRAINT [FK_Transaction_Sale]
GO


/****** Object:  Table [dbo].[TransactionProperty]    Script Date: 30-01-2021 11:50:46 ******/

CREATE TABLE [dbo].[TransactionProperty](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TransactionId] [int] NOT NULL,
	[PropertyName] [varchar](150) NOT NULL,
	[PropertyValue] [varchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TransactionProperty]  WITH CHECK ADD  CONSTRAINT [FK_TransactionProperty_Transaction] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[Transaction] ([Id])
GO

ALTER TABLE [dbo].[TransactionProperty] CHECK CONSTRAINT [FK_TransactionProperty_Transaction]
GO


