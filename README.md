[![NuGet](https://img.shields.io/nuget/v/DataTables.ServerSideProcessing.EFCore.svg)](https://www.nuget.org/packages/DataTables.ServerSideProcessing.EFCore)

# DataTables.ServerSideProcessing.EFCore

A small .NET library for handling [jQuery DataTables](https://datatables.net/) **server-side processing** requests with **Entity Framework Core**.

You give it an `IQueryable<T>` and the request form that DataTables sends, and it returns a response DataTables understands. Because it is built on top of EF Core's `IQueryable`, searching, filtering, sorting and paging are applied directly at the database level, and only the requested data is retrieved. This is particularly useful for large datasets, where client-side processing would be inefficient or impractical.

## Features

- Paging (`start` / `length`, including `length = -1` for "all rows")
- Multi-column sorting
- Global search across the columns you choose
- Column filters: text, numeric, date, single-select and multi-select
- Works with any EF Core `IQueryable<T>`, with or without projection to a DTO
- Async and sync APIs
- Footer data support (totals, summaries)
- Targets .NET 8, 9 and 10. Tested with SQL Server, PostgreSQL and MySQL.

## Installation

```bash
dotnet add package DataTables.ServerSideProcessing.EFCore
```

## Quick start

**Controller**

```csharp
using DataTables.ServerSideProcessing.EFCore;

[HttpPost]
public async Task<IActionResult> GetPeople(CancellationToken ct)
{
    var response = await _db.People
        .ForDataTable(Request.Form, p => new PersonDto
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName,
            BirthDate = p.BirthDate
        })
        .WithGlobalFilter(nameof(PersonDto.FirstName), nameof(PersonDto.LastName))
        .BuildAsync(ct);

    return Json(response);
}
```

**JavaScript**

```js
new DataTable('#people', {
    serverSide: true,
    processing: true,
    ajax: { url: '/People/GetPeople', type: 'POST' },
    columns: [
        { data: 'firstName' },
        { data: 'lastName' },
        { data: 'birthDate' }
    ]
});
```

The `data` value of each column is the name of the property used for sorting (case-insensitive). The response is serialized with ASP.NET Core's default camelCase JSON settings into the `draw`, `recordsTotal`, `recordsFiltered` and `data` fields DataTables expects.

This quick start covers paging, sorting and global search. Column filters need extra fields sent from the client (see [Column filters](#column-filters)).

The projection is optional. Without it, call `.ForDataTable(Request.Form)`, and the response contains the query's own type (for example the entity).

When a projection is used, search, filters and sorting are applied **after** the projection. Property names sent from the client must therefore be **property names of the projected type (the DTO)**, not of the entity. This keeps the table tied to the model you display, independent of the database model.

## Builder options

| Method | Description |
|---|---|
| `WithGlobalFilter(params string[] properties)` | Applies the DataTables search box to the given properties. Off by default. |
| `WithoutSorting()` | Ignores sorting sent by DataTables. |
| `WithoutColumnFilters()` | Ignores column filters. |

## Column filters

The library only reads column filters from the request. It doesn't include a filter UI or client-side code for them. How you build the filter inputs and collect their values is up to you.

Filters are sent in the same request as the standard DataTables parameters, as additional form fields (for example, added in [`ajax.data`](https://datatables.net/reference/option/ajax.data)). Each filtered property needs these fields:

```text
filter[birthDate]                   = 01/01/2000
filter[birthDate][filterCategory]   = Date
filter[birthDate][filterType]       = GreaterThanOrEqual
filter[birthDate][valueCategory]    = (Text and Numeric filters only)
```

Filters with an empty value are skipped.

| `filterCategory` | `filterType` | `valueCategory` | Value |
|---|---|---|---|
| `Text` | `Equals`, `NotEqual`, `Contains`, `DoesNotContain`, `StartsWith`, `EndsWith` | `Base` or `AccNumber` (strips `-` from the search value) | text |
| `Numeric` | `Equals`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `Between` | `Int` for `int` properties, `Decimal` for `decimal` properties | number, or `from;to` for `Between` |
| `Date` | same as `Numeric` | – | date, or `from;to` for `Between` |
| `SingleSelect` | – | – | single value |
| `MultiSelect` | – | – | comma-separated values: `a,b,c` |

Notes:
- `filterCategory`, `filterType` and `valueCategory` accept either the enum name (`Date`) or its numeric value (`2`).
- For `Between`, either side can be left empty: `;100` means "up to 100", and `10;` means "10 or more".
- Date filters work with `DateOnly`, `DateTime` and `DateTimeOffset` properties. For `DateTime` and `DateTimeOffset`, `Equals` matches the whole day.
- Select filters work with strings, numbers, dates, enums (by name) and booleans. Values that can't be converted to the property type are ignored.
- Numbers and dates are parsed using the current culture (`CultureInfo.CurrentCulture`).

The prefix (`filter`), the key names (`filterCategory`, `filterType`, `valueCategory`) and the separators (`;`, `,`) can be changed by passing a `FilterParsingOptions` instance. The structure `prefix[property]` / `prefix[property][key]` stays the same. For example:

```csharp
.BuildAsync(new FilterParsingOptions { Prefix = "f", MultiSelectSeparator = "|" }, ct);
```

The `*FilterComponent` records (`TextFilterComponent`, `NumericFilterComponent`, `DateFilterComponent`, `SelectFilterComponent`, `MultiSelectFilterComponent`) and `DataTableComponent` are optional helper models for describing a table's filters (column, filter category, initial operation, select options), for example to render the filter row in a Razor partial view.

## Advanced usage

### Custom logic on top of the generated query

The builder exposes each stage of the query, so you can add your own conditions or calculations:

| Method | Returns |
|---|---|
| `GetBaseQuery()` | the source query with the projection applied |
| `GetFilteredQuery()` | the source query plus global and column filters |
| `GetFinalQuery()` | the source query plus filters and sorting |
| `BuildFromQueryAsync(query)` | a DataTables response built from your own query (counts and paging are handled for you) |

```csharp
var builder = _db.Orders.ForDataTable(Request.Form, OrderMappings.ToDto);

var query = builder.GetFinalQuery()
                   .Where(o => o.Status != OrderStatus.Cancelled);

var response = await builder.BuildFromQueryAsync(query, ct);
```

### Footer data

```csharp
var builder = _db.Orders.ForDataTable(Request.Form, OrderMappings.ToDto);

decimal total = await builder.GetFilteredQuery().SumAsync(o => o.Amount, ct);
var response = await builder.BuildAsync(ct);

return Json(response.Extend(new List<decimal> { total }));
```

### Data only (e.g. export)

`GetDataAsync()` applies the same filters, sorting and paging but returns only the list, without counts. Paging is taken from the request (`start` / `length`), so if the client sends `length = -1`, all rows are returned. To ignore the paging sent by the client, pass your own values: `GetDataAsync(0, -1, ct)` returns all rows.

### Sync API

Each async method has a sync version: `Build()`, `GetData()` and `BuildFromQuery()`.

## Links

- [DataTables server-side processing](https://datatables.net/manual/server-side): the request and response format
- [DataTables `ajax.data` option](https://datatables.net/reference/option/ajax.data): sending extra parameters such as column filters
