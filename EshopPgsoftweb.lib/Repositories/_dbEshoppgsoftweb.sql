/****** Object:  Table [dbo].[epUserProp] ******/
CREATE TABLE [dbo].[epUserProp](
	[pk] [uniqueidentifier] NOT NULL,
	[dateCreate] [datetime] NOT NULL,
	[userId] [int] NULL,
	[sessionId] [nvarchar](255) NULL,
	[propId] [nvarchar](255) NOT NULL,
	[propValue] [ntext] NULL,
 CONSTRAINT [PK_epUserProp] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epCountry] ******/
CREATE TABLE [dbo].[epCountry](
	[pk] [uniqueidentifier] NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_epCountry] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epCustomer] ******/
CREATE TABLE [dbo].[epCustomer](
	[pk] [uniqueidentifier] NOT NULL,
	[ownerId] [int] NOT NULL,

	[name] [nvarchar](255) NOT NULL,
	[countryKey] [uniqueidentifier] NOT NULL,
	[street] [nvarchar](255) NOT NULL,
	[city] [nvarchar](255) NOT NULL,
	[zip] [nvarchar](10) NOT NULL,
	[phone] [nvarchar](255) NOT NULL,
	[email] [nvarchar](255) NOT NULL,

	[ico] [nvarchar](50) NULL,
	[dic] [nvarchar](50) NULL,
	[icdph] [nvarchar](50) NULL,
	[contactName] [nvarchar](255) NULL,

	[isDeliveryAddress] [bit] NOT NULL,
	[deliveryName] [nvarchar](255) NULL,
	[deliveryCountryKey] [uniqueidentifier] NULL,
	[deliveryStreet] [nvarchar](255) NULL,
	[deliveryCity] [nvarchar](255) NULL,
	[deliveryZip] [nvarchar](10) NULL,
	[deliveryPhone] [nvarchar](255) NULL,

CONSTRAINT [PK_epCustomer] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epCustomer]  WITH CHECK ADD  CONSTRAINT [FK_epCustomer_countryKey] FOREIGN KEY([countryKey])
REFERENCES [dbo].[epCountry] ([pk])
GO
ALTER TABLE [dbo].[epCustomer] CHECK CONSTRAINT [FK_epCustomer_countryKey]
GO
ALTER TABLE [dbo].[epCustomer]  WITH CHECK ADD  CONSTRAINT [FK_epCustomer_deliveryCountryKey] FOREIGN KEY([deliveryCountryKey])
REFERENCES [dbo].[epCountry] ([pk])
GO
ALTER TABLE [dbo].[epCustomer] CHECK CONSTRAINT [FK_epCustomer_deliveryCountryKey]
GO

/****** Object:  Table [dbo].[epProduct2CustomerFavorite] ******/
CREATE TABLE [dbo].[epProduct2CustomerFavorite](
	[pk] [uniqueidentifier] NOT NULL,
	[pkCustomer] [uniqueidentifier] NOT NULL,
	[pkProduct] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_epProduct2CustomerFavorite] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProduct2CustomerFavorite]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2CustomerFavorite_epCustomer] FOREIGN KEY([pkCustomer])
REFERENCES [dbo].[epCustomer] ([pk]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[epProduct2CustomerFavorite]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2CustomerFavorite_epProduct] FOREIGN KEY([pkProduct])
REFERENCES [dbo].[epProduct] ([pk]) ON DELETE CASCADE
GO


/****** Object:  Table [dbo].[epProducer] ******/
CREATE TABLE [dbo].[epProducer](
	[pk] [uniqueidentifier] NOT NULL,
	[producerName] [nvarchar](255) NOT NULL,
	[producerDescription] [ntext] NULL,
	[producerWeb] [nvarchar](255) NULL
 CONSTRAINT [PK_epProducer] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epAvailability] ******/
CREATE TABLE [dbo].[epAvailability](
	[pk] [uniqueidentifier] NOT NULL,
	[availabilityName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_epAvailability] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epSysConst] ******/
CREATE TABLE [dbo].[epSysConst](
	[pk] [uniqueidentifier] NOT NULL,

	[companyName] [nvarchar](255) NOT NULL,
	[companyIco] [nvarchar](50) NOT NULL,
	[companyDic] [nvarchar](50) NOT NULL,
	[companyIcdph] [nvarchar](50) NOT NULL,

	[addressStreet] [nvarchar](255) NOT NULL,
	[addressCity] [nvarchar](255) NOT NULL,
	[addressZip] [nvarchar](10) NOT NULL,

	[email] [nvarchar](255) NOT NULL,
	[phone] [nvarchar](255) NOT NULL,

	[bank] [nvarchar](255) NOT NULL,
	[iban] [nvarchar](255) NOT NULL,
	[currency] [nvarchar](10) NOT NULL,
	[freeTransportPrice] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_epSysConst] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epTransportType] ******/
CREATE TABLE [dbo].[epTransportType](
	[pk] [uniqueidentifier] NOT NULL,
	[transportOrder] [int] NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[priceNoVat] [decimal](18, 2) NOT NULL,
	[priceWithVat] [decimal](18, 2) NOT NULL,
	[vatPerc] [decimal](18, 2) NOT NULL,
	[gatewayTypeId] [int] NOT NULL,
 CONSTRAINT [PK_epTransportType] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epPaymentType] ******/
CREATE TABLE [dbo].[epPaymentType](
	[pk] [uniqueidentifier] NOT NULL,
	[paymentOrder] [int] NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[priceNoVat] [decimal](18, 2) NOT NULL,
	[priceWithVat] [decimal](18, 2) NOT NULL,
	[vatPerc] [decimal](18, 2) NOT NULL,
	[gatewayTypeId] [int] NOT NULL,
 CONSTRAINT [PK_epPaymentType] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epQuoteState] ******/
CREATE TABLE [dbo].[epQuoteState](
	[pk] [uniqueidentifier] NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[title] [nvarchar](255) NOT NULL,
	[exportToMksoft] [bit] NOT NULL,
 CONSTRAINT [PK_epQuoteState] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epPaymentState] ******/
CREATE TABLE [dbo].[epPaymentState](
	[pk] [uniqueidentifier] NOT NULL,
	[code] [nvarchar](50) NOT NULL,
	[title] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_epPaymentState] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epFileDescription] ******/
CREATE TABLE [dbo].[epFileDescription](
	[pk] [uniqueidentifier] NOT NULL,
	[category] [nvarchar](255) NOT NULL,
	[fileName] [nvarchar](255) NOT NULL,
	[description] [nvarchar](255) NULL,
 CONSTRAINT [PK_epFileDescription] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[epProductAttribute] ******/
CREATE TABLE [dbo].[epProductAttribute](
	[pk] [uniqueidentifier] NOT NULL,
	[productAttributeOrder] [int] NOT NULL,
	[productAttributeGroup] [nvarchar](255) NOT NULL,
	[productAttributeName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_epProductAttribute] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epCategory] ******/
CREATE TABLE [dbo].[epCategory](
	[pk] [uniqueidentifier] NOT NULL,
	[categoryIsVisible] [bit] NOT NULL,
	[parentCategoryKey] [uniqueidentifier] NOT NULL,
	[categoryOrder] [int] NOT NULL,
	[categoryCode] [nvarchar](50) NOT NULL,
	[categoryName] [nvarchar](255) NOT NULL,
	[categoryDescription] [ntext] NULL,
	[categoryImg] [nvarchar](255) NULL,
	[categoryOfferText] [ntext] NULL,
	[categoryUrl] [nvarchar](255) NOT NULL,
	[categoryMetaTitle] [nvarchar](255) NOT NULL,
	[categoryMetaKeywords] [nvarchar](255) NOT NULL,
	[categoryMetaDescription] [nvarchar](255) NOT NULL
 CONSTRAINT [PK_epCategory] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[epProduct] ******/
CREATE TABLE [dbo].[epProduct](
	[pk] [uniqueidentifier] NOT NULL,
	[productIsVisible] [bit] NOT NULL,
	[producerKey] [uniqueidentifier] NOT NULL,
	[productCode] [nvarchar](50) NOT NULL,
	[productName] [nvarchar](255) NOT NULL,
	[productText] [ntext] NOT NULL,
	[productDescription] [ntext] NULL,
	[productOrder] [int] NOT NULL,
	[productImg] [nvarchar](255) NULL,
	[productUrl] [nvarchar](255) NOT NULL,
	[productMetaTitle] [nvarchar](255) NOT NULL,
	[productMetaKeywords] [nvarchar](255) NOT NULL,
	[productMetaDescription] [nvarchar](255) NOT NULL,

	[availabilityKey] [uniqueidentifier] NOT NULL,
	[unitTypeId] [int] NOT NULL,
	[productIsNew] [bit] NOT NULL,
	[productIsSale] [bit] NOT NULL,
	[productDurability] [nvarchar](50) NULL,
	[productUnitWeight] [decimal](18, 2) NOT NULL,
	[productUnitsInPckg] [decimal](18, 2) NOT NULL,
	[productCountry] [nvarchar](255) NULL,
 CONSTRAINT [PK_epProduct] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProduct]  WITH CHECK ADD  CONSTRAINT [FK_epProduct_producerKey] FOREIGN KEY([producerKey])
REFERENCES [dbo].[epProducer] ([pk])
GO
ALTER TABLE [dbo].[epProduct] CHECK CONSTRAINT [FK_epProduct_producerKey]
GO
ALTER TABLE [dbo].[epProduct]  WITH CHECK ADD  CONSTRAINT [FK_epProduct_availabilityKey] FOREIGN KEY([availabilityKey])
REFERENCES [dbo].[epAvailability] ([pk])
GO
ALTER TABLE [dbo].[epProduct] CHECK CONSTRAINT [FK_epProduct_availabilityKey]
GO

/****** Object:  Table [dbo].[epProductPrice] ******/
CREATE TABLE [dbo].[epProductPrice](
	[pk] [uniqueidentifier] NOT NULL,
	[productKey] [uniqueidentifier] NOT NULL,
	[validFrom] [datetime] NOT NULL,
	[validTo] [datetime] NULL,
	[vatRate] [decimal](18, 2) NOT NULL,
	[price_1_NoVat] [decimal](18, 2) NOT NULL,
	[price_1_WithVat] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_epProductPrice] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProductPrice]  WITH CHECK ADD  CONSTRAINT [FK_epProductPrice_productKey] FOREIGN KEY([productKey])
REFERENCES [dbo].[epProduct] ([pk])
GO
ALTER TABLE [dbo].[epProductPrice] CHECK CONSTRAINT [FK_epProductPrice_productKey]
GO


/****** Object:  Table [dbo].[epProduct2Category] ******/
CREATE TABLE [dbo].[epProduct2Category](
	[pkCategory] [uniqueidentifier] NOT NULL,
	[pkProduct] [uniqueidentifier] NOT NULL,
	[productOrder] [int] NOT NULL
 CONSTRAINT [PK_epProduct2Category] PRIMARY KEY CLUSTERED 
(
	[pkProduct] ASC,
	[pkCategory] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProduct2Category]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2Category_pkProduct] FOREIGN KEY([pkProduct])
REFERENCES [dbo].[epProduct] ([pk])
GO
ALTER TABLE [dbo].[epProduct2Category] CHECK CONSTRAINT [FK_epProduct2Category_pkProduct]
GO
ALTER TABLE [dbo].[epProduct2Category]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2Category_pkCategory] FOREIGN KEY([pkCategory])
REFERENCES [dbo].[epCategory] ([pk])
GO
ALTER TABLE [dbo].[epProduct2Category] CHECK CONSTRAINT [FK_epProduct2Category_pkCategory]
GO

/****** Object:  Table [dbo].[epProduct2Attribute] ******/
CREATE TABLE [dbo].[epProduct2Attribute](
	[pkAttribute] [uniqueidentifier] NOT NULL,
	[pkProduct] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_epProduct2Attribute] PRIMARY KEY CLUSTERED 
(
	[pkProduct] ASC,
	[pkAttribute] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProduct2Attribute]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2Attribute_pkProduct] FOREIGN KEY([pkProduct])
REFERENCES [dbo].[epProduct] ([pk])
GO
ALTER TABLE [dbo].[epProduct2Attribute] CHECK CONSTRAINT [FK_epProduct2Attribute_pkProduct]
GO
ALTER TABLE [dbo].[epProduct2Attribute]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2Attribute_pkAttribute] FOREIGN KEY([pkAttribute])
REFERENCES [dbo].[epProductAttribute] ([pk])
GO
ALTER TABLE [dbo].[epProduct2Attribute] CHECK CONSTRAINT [FK_epProduct2Attribute_pkAttribute]
GO

/****** Object:  Table [dbo].[epProductRelation] ******/
CREATE TABLE [dbo].[epProductRelation](
	[pkProductMain] [uniqueidentifier] NOT NULL,
	[pkProductRelated] [uniqueidentifier] NOT NULL
 CONSTRAINT [PK_epProductRelation] PRIMARY KEY CLUSTERED 
(
	[pkProductMain] ASC,
	[pkProductRelated] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProductRelation]  WITH CHECK ADD  CONSTRAINT [FK_epProductRelation_pkProductMain] FOREIGN KEY([pkProductMain])
REFERENCES [dbo].[epProduct] ([pk])
GO
ALTER TABLE [dbo].[epProductRelation] CHECK CONSTRAINT [FK_epProductRelation_pkProductMain]
GO
GO
ALTER TABLE [dbo].[epProductRelation]  WITH CHECK ADD  CONSTRAINT [FK_epProductRelation_pkProductRelated] FOREIGN KEY([pkProductRelated])
REFERENCES [dbo].[epProduct] ([pk])
GO
ALTER TABLE [dbo].[epProductRelation] CHECK CONSTRAINT [FK_epProductRelation_pkProductRelated]
GO


/****** Object:  Table [dbo].[epQuote] ******/
CREATE TABLE [dbo].[epQuote](
	[pk] [uniqueidentifier] NOT NULL,
	[dateCreate] [datetime] NOT NULL,
	[dateFinished] [datetime] NULL,
	[sessionId] [nvarchar](255) NULL,
	[finishedSessionId] [nvarchar](255) NULL,
	[quoteYear] [int] NULL,
	[quoteNumber] [int] NULL,
	[quotePriceNoVat] [decimal](18, 2) NOT NULL,
	[quotePriceWithVat] [decimal](18, 2) NOT NULL,
	[quoteState] [nvarchar](255),
	[quotePriceState] [nvarchar](255),
	[note] [ntext] NULL,
 CONSTRAINT [PK_epQuote] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[epProduct2Quote] ******/
CREATE TABLE [dbo].[epProduct2Quote](
	[pk] [uniqueidentifier] NOT NULL,
	[pkQuote] [uniqueidentifier] NOT NULL,
	[pkProduct] [uniqueidentifier] NOT NULL,
	[nonProductId] [nvarchar](255) NULL,
	[itemOrder] [int] NOT NULL,
	[itemPcs] [decimal](18, 3) NOT NULL,
	[unitWeight] [decimal](18, 2) NOT NULL,
	[unitPriceNoVat] [decimal](18, 2) NOT NULL,
	[unitPriceWithVat] [decimal](18, 2) NOT NULL,
	[vatPerc] [decimal](18, 2) NOT NULL,
	[itemCode] [nvarchar](50) NOT NULL,
	[itemName] [nvarchar](255) NOT NULL,
	[unitName] [nvarchar](50) NULL,
	[unitTypeId] [int] NOT NULL,
 CONSTRAINT [PK_epProduct2Quote] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epProduct2Quote]  WITH CHECK ADD  CONSTRAINT [FK_epProduct2Quote_epQuote] FOREIGN KEY([pkQuote])
REFERENCES [dbo].[epQuote] ([pk]) ON DELETE CASCADE
GO

/****** Object:  Table [dbo].[epUser2Quote] ******/
CREATE TABLE [dbo].[epUser2Quote](
	[pk] [uniqueidentifier] NOT NULL,
	[pkQuote] [uniqueidentifier] NOT NULL,
	[pkUser] [uniqueidentifier] NOT NULL,

	[isCompanyInvoice] [bit] NOT NULL,
	[companyName] [nvarchar](255) NULL,
	[companyIco] [nvarchar](50) NULL,
	[companyDic] [nvarchar](50) NULL,
	[companyIcdph] [nvarchar](50) NULL,

	[invName] [nvarchar](255) NOT NULL,
	[invStreet] [nvarchar](255) NOT NULL,
	[invCity] [nvarchar](255) NOT NULL,
	[invZip] [nvarchar](10) NOT NULL,
	[invCountry] [nvarchar](255) NOT NULL,

	[isDeliveryAddress] [bit] NOT NULL,
	[deliveryName] [nvarchar](255) NULL,
	[deliveryStreet] [nvarchar](255) NULL,
	[deliveryCity] [nvarchar](255) NULL,
	[deliveryZip] [nvarchar](10) NULL,
	[deliveryCountry] [nvarchar](255) NULL,

	[quoteEmail] [nvarchar](255) NOT NULL,
	[quotePhone] [nvarchar](255) NOT NULL,

	[note] [ntext] NULL,
 CONSTRAINT [PK_epUser2Quote] PRIMARY KEY CLUSTERED 
(
	[pk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[epUser2Quote]  WITH CHECK ADD  CONSTRAINT [FK_epUser2Quote_epQuote] FOREIGN KEY([pkQuote])
REFERENCES [dbo].[epQuote] ([pk]) ON DELETE CASCADE
GO
