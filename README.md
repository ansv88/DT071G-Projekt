#  Konsolapp för generering av slumpade veckomatsedlar

Detta projekt är en C#-konsolapplikation som använder LiteDB för lokal datalagring.

## Installation

1. **Klona** detta repository.
2. **Öppna** projektet i Visual Studio.
3. **NuGet-paket**: Om LiteDB inte redan är installerat, installera det genom att köra `Install-Package LiteDB` i NuGet Package Manager Console.

## Databas

Programmet använder LiteDB och skapar automatiskt en databasfil (`database.db`) i mappen `Data` vid körning. Om `database.db` saknas kommer programmet att generera den och fylla den med grunddata från `dishes.json`.

## Seed-fil

För grunddata: Programmet letar efter `dishes.json` i `Data`-mappen och läser in data om databasen är tom. Se till att `dishes.json` finns i `Data` för korrekt initialisering.
