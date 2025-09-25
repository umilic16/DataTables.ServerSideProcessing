## 🛠 In Progress

  - [ ] Fix global filter and select filter for enum/date types
  - [ ] Cache whole properties instead just names, so reflection is not used multiple times

## 🔥 High priority / Upcoming

  - [ ] Add support for building query (and accessing it) but without executing it
  - [ ] Select/MultiSelect generic (instead of just string) support
  - [ ] CI/CD
  - [ ] Component model for whole header/table

## 💡 Low priority / Ideas

  - [ ] Better way of accessing property names (e.g. by some data annotation building with source generator or some method)
  - [ ] Maybe some data annotation on properties which should be included in global filtering
  - [ ] Support for caching
  - [ ] Add examples, improve documentation
  - [ ] Add global configuration support (e.g. modifying Default instance of FilterParsingOptions, RequestParser...)
  - [ ] Custom exceptions
  - [ ] Js helper methods
  - [ ] Add components?
  - [ ] Improve how data is sent from the frontend
  - [ ] Add logging warnings

## 🧹 Done

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
