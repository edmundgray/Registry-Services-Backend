USE [master]
GO
/****** Object:  Database [RegistryDatabase]    Script Date: 11/05/2025 12:53:31 ******/
CREATE DATABASE [RegistryDatabase]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'RegistryDatabase', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RegistryDatabase.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'RegistryDatabase_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RegistryDatabase_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [RegistryDatabase] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [RegistryDatabase].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [RegistryDatabase] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [RegistryDatabase] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [RegistryDatabase] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [RegistryDatabase] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [RegistryDatabase] SET ARITHABORT OFF 
GO
ALTER DATABASE [RegistryDatabase] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [RegistryDatabase] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [RegistryDatabase] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [RegistryDatabase] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [RegistryDatabase] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [RegistryDatabase] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [RegistryDatabase] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [RegistryDatabase] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [RegistryDatabase] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [RegistryDatabase] SET  DISABLE_BROKER 
GO
ALTER DATABASE [RegistryDatabase] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [RegistryDatabase] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [RegistryDatabase] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [RegistryDatabase] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [RegistryDatabase] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [RegistryDatabase] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [RegistryDatabase] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [RegistryDatabase] SET RECOVERY FULL 
GO
ALTER DATABASE [RegistryDatabase] SET  MULTI_USER 
GO
ALTER DATABASE [RegistryDatabase] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [RegistryDatabase] SET DB_CHAINING OFF 
GO
ALTER DATABASE [RegistryDatabase] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [RegistryDatabase] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [RegistryDatabase] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [RegistryDatabase] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'RegistryDatabase', N'ON'
GO
ALTER DATABASE [RegistryDatabase] SET QUERY_STORE = ON
GO
ALTER DATABASE [RegistryDatabase] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [RegistryDatabase]
GO
/****** Object:  Table [dbo].[CoreInvoiceModel]    Script Date: 11/05/2025 12:53:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoreInvoiceModel](
	[Mandatory] [varchar](10) NULL,
	[ID] [varchar](10) NOT NULL,
	[Level] [varchar](10) NOT NULL,
	[Cardinality] [varchar](20) NOT NULL,
	[BusinessTerm] [varchar](255) NOT NULL,
	[SemanticDescription] [text] NULL,
	[UsageNote] [text] NULL,
	[DataType] [varchar](50) NULL,
	[BusinessRules] [varchar](50) NULL,
	[ReqID] [varchar](50) NULL,
	[ParentID] [varchar](50) NULL,
 CONSTRAINT [PK_CoreInvoiceModel] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExtensionComponentModelElements]    Script Date: 11/05/2025 12:53:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExtensionComponentModelElements](
	[EntityID] [int] IDENTITY(1,1) NOT NULL,
	[ExtensionComponentID] [varchar](10) NOT NULL,
	[BusinessTermID] [varchar](10) NOT NULL,
	[Level] [varchar](10) NULL,
	[Cardinality] [varchar](20) NULL,
	[BusinessTerm] [varchar](255) NOT NULL,
	[SemanticDescription] [text] NULL,
	[UsageNoteCore] [text] NULL,
	[UsageNoteExtension] [text] NULL,
	[Justification] [text] NULL,
	[DataType] [varchar](50) NULL,
	[ExtensionType] [varchar](50) NULL,
	[ConformanceType] [varchar](50) NULL,
	[ParentID] [varchar](50) NULL,
 CONSTRAINT [PK_ExtensionComponentElements] PRIMARY KEY CLUSTERED 
(
	[EntityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExtensionComponentsModelHeader]    Script Date: 11/05/2025 12:53:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExtensionComponentsModelHeader](
	[ID] [varchar](10) NOT NULL,
	[Name] [nchar](100) NOT NULL,
	[Status] [nchar](50) NULL,
	[ECLink] [nvarchar](max) NULL,
 CONSTRAINT [PK_ExtensionComponents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SpecificationCore]    Script Date: 11/05/2025 12:53:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpecificationCore](
	[EntityID] [int] IDENTITY(1,1) NOT NULL,
	[IdentityID] [int] NOT NULL,
	[BusinessTermID] [varchar](10) NOT NULL,
	[Cardinality] [varchar](20) NOT NULL,
	[UsageNote] [nchar](10) NULL,
	[TypeOfChange] [text] NOT NULL,
 CONSTRAINT [PK_SpecificationCore] PRIMARY KEY CLUSTERED 
(
	[EntityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SpecificationExtensionComponents]    Script Date: 11/05/2025 12:53:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpecificationExtensionComponents](
	[EntityID] [int] IDENTITY(1,1) NOT NULL,
	[IdentityID] [int] NOT NULL,
	[ExtensionComponentID] [varchar](10) NULL,
	[BusinessTermID] [varchar](10) NOT NULL,
	[Cardinality] [varchar](20) NOT NULL,
	[UsageNote] [text] NULL,
	[Justification] [text] NULL,
	[TypeOfExtension] [varchar](50) NOT NULL,
 CONSTRAINT [PK_SpecificationExtensionComponents2] PRIMARY KEY CLUSTERED 
(
	[EntityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SpecificationIdentifyingInformation]    Script Date: 11/05/2025 12:53:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpecificationIdentifyingInformation](
	[IdentityID] [int] IDENTITY(1,1) NOT NULL,
	[SpecificationIdentifier] [nvarchar](255) NOT NULL,
	[SpecificationName] [nvarchar](max) NOT NULL,
	[Sector] [nvarchar](200) NOT NULL,
	[SubSector] [nvarchar](200) NULL,
	[Purpose] [nvarchar](max) NOT NULL,
	[SpecificationVersion] [nvarchar](50) NULL,
	[ContactInformation] [nvarchar](max) NOT NULL,
	[DateOfImplementation] [date] NULL,
	[GoverningEntity] [nvarchar](max) NULL,
	[CoreVersion] [nvarchar](50) NULL,
	[SpecificationSourceLink] [nvarchar](255) NULL,
	[Country] [nvarchar](200) NULL,
	[IsCountrySpecification] [bit] NOT NULL,
	[UnderlyingSpecificationIdentifier] [nvarchar](255) NULL,
	[PreferredSyntax] [nvarchar](100) NULL,
 CONSTRAINT [PK_SpecificationIdentifyingInformation] PRIMARY KEY CLUSTERED 
(
	[IdentityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ExtensionComponentModelElements]  WITH CHECK ADD  CONSTRAINT [FK_ExtensionComponentElements_ExtensionComponents] FOREIGN KEY([ExtensionComponentID])
REFERENCES [dbo].[ExtensionComponentsModelHeader] ([ID])
GO
ALTER TABLE [dbo].[ExtensionComponentModelElements] CHECK CONSTRAINT [FK_ExtensionComponentElements_ExtensionComponents]
GO
ALTER TABLE [dbo].[SpecificationCore]  WITH CHECK ADD  CONSTRAINT [FK_SpecificationCore_CoreInvoiceModel] FOREIGN KEY([BusinessTermID])
REFERENCES [dbo].[CoreInvoiceModel] ([ID])
GO
ALTER TABLE [dbo].[SpecificationCore] CHECK CONSTRAINT [FK_SpecificationCore_CoreInvoiceModel]
GO
ALTER TABLE [dbo].[SpecificationCore]  WITH CHECK ADD  CONSTRAINT [FK_SpecificationCore_SpecificationIdentifyingInformation] FOREIGN KEY([IdentityID])
REFERENCES [dbo].[SpecificationIdentifyingInformation] ([IdentityID])
GO
ALTER TABLE [dbo].[SpecificationCore] CHECK CONSTRAINT [FK_SpecificationCore_SpecificationIdentifyingInformation]
GO
ALTER TABLE [dbo].[SpecificationExtensionComponents]  WITH CHECK ADD  CONSTRAINT [FK_SpecificationExtensionComponents_SpecificationIdentifyingInformation] FOREIGN KEY([IdentityID])
REFERENCES [dbo].[SpecificationIdentifyingInformation] ([IdentityID])
GO
ALTER TABLE [dbo].[SpecificationExtensionComponents] CHECK CONSTRAINT [FK_SpecificationExtensionComponents_SpecificationIdentifyingInformation]
GO
USE [master]
GO
ALTER DATABASE [RegistryDatabase] SET  READ_WRITE 
GO
