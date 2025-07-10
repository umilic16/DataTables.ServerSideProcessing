[![NuGet](https://img.shields.io/nuget/v/DataTables.ServerSideProcessing.EFCore.svg)](https://www.nuget.org/packages/DataTables.ServerSideProcessing.EFCore)

# DataTables.ServerSideProcessing.EFCore

A .NET library for efficient **server-side processing** of [jQuery DataTables](https://datatables.net/) using **Entity Framework Core**.

Provides a flexible solution for handling DataTables requests, with support for multi-column sorting, advanced filtering, and seamless integration with EF Core’s `IQueryable`.

## 🚀 Integration with EF Core

Built on top of EF Core's IQueryable, this library retrieves only the data required for the current page, applying filters, sorting, and pagination directly at the database level. 
This is particularly useful for large datasets where client-side processing would be inefficient or impractical.

## ✨ Features

- **Multi-column sorting**: Order by multiple columns, each with its own direction (ascending/descending).
- **Multiple filter types**: Text, number, date, and select/multi-select filters supported.
- **Column-specific filtering**: Filter by column using a variety of operators (`Contains`, `Equals`, `StartsWith`, `GreaterThan`, etc.) depending on the filter type.
- **Global search**: Apply a search across all (specified) columns.
- **Pagination**: Efficiently skip and take records for DataTables paging.
- **Strongly-typed models**: Request and response models for type safety and clarity.
- **Easy integration**: Seamlessly integrate into your ASP.NET Core applications.
- **Async and sync APIs**: Choose between asynchronous or synchronous methods to fit your application's needs.
- **NuGet package**: [DataTables.ServerSideProcessing.EFCore](https://www.nuget.org/packages/DataTables.ServerSideProcessing.EFCore)

## ⚡ Quick Start

_Coming soon!_  
A step-by-step guide and practical examples will be added to help you get up and running quickly.
