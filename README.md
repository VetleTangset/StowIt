# StowIt

A simple C# app for keeping track of what's in your storage boxes, and where those boxes are.

I built it to answer one question: *"Which box is my blue wool sweater in?"*

## Features

- Create boxes with a label and a location (Apartment or Basement)
- Put items in boxes and tag them with one or more categories (Tools, Electronics, Seasonal, ...)
- Search for items by words in any order, e.g. `blue wool sweater`
- List all items in a category

### Planned

- [ ] Save and load data (SQL)
- [ ] Console front end
- [ ] Unit tests
- [ ] Graphical front end

## Project structure

```
StowIt/
├── StowIt.Core/      Class library with the core logic (no UI code)
│   ├── Box.cs
│   ├── Item.cs
│   ├── Category.cs
│   ├── StorageArea.cs
│   └── StorageService.cs
└── StowIt.Test/      Testing of functions in library
└── StowIt.App/       Front end that uses the library (coming soon)
```

The library only returns data. Any front end (console, WPF, web) decides how to display it.

## Getting started

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer.

```
git clone https://github.com/VetleTangset/StowIt.git
cd StowIt
dotnet build
```

## License

Released under the [![License: Apache-2.0](https://img.shields.io/badge/License-Apache--2.0-yellow.svg)](https://opensource.org/licenses/Apache-2.0)
