## 🛠 In Progress


## 🔥 High priority / Upcoming

  - [ ] CI/CD

## 💡 Low priority / Ideas

  - [ ] Fix global filter (in .net 9 only DateOnly breaks on non mssql providers)
  - [ ] Better way of accessing property names (e.g. by some data annotation building with source generator or some method)
  - [ ] Maybe some data annotation on properties which should be included in global filtering
  - [ ] Support for caching
  - [ ] Add examples, improve documentation
  - [ ] Custom exceptions
  - [ ] Js helper methods
  - [ ] Add components?
  - [ ] Improve how data is sent from the frontend
  - [ ] Add logging warnings

## 🧹 Done

  - [x] Add global configuration support (e.g. modifying Default instance of FilterParsingOptions, RequestParser...)
  - [x] Add support for building query (and accessing it) but without executing it
  - [x] DI config of default options
  - [x] Component model for whole header/table
  - [x] Fix select/multiselect filtering for enums, date types
  - [x] Cache whole properties instead just names
  - [x] Cache methods that are being retrieved via reflection
  - [x] Add tests
  - [x] Fix filtering dates when the property that is filtered on is datetime
  - [x] DateTimeOffset support
  - [x] Switch data models to records
  - [x] Method to return just data instead of whole response object
  - [x] Add a way of accessing built query (from different stages)
  - [x] Refactor ResponseBuilder to Fluent API
  - [x] Refactor models, enums
  - [x] Add separator choice for 'Between' filter
  - [x] Filters are enumerated multiple times, switch to different collection type
  - [x] Add DateTime range filtering
  - [x] Documentation
  - [x] Central package management
  - [x] Static Code analysis
  - [x] More helper methods that simplify usage
  - [x] List based filter
  - [x] Seal classes
