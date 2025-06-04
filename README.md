# WarsztatSamochodowyApp

Aplikacja webowa do zarządzania częściami samochodowymi w warsztacie, stworzona w technologii ASP.NET Core MVC.

## Funkcjonalności

- Przeglądanie listy części samochodowych
- Dodawanie, edytowanie i usuwanie części
- Przypisywanie części do typów części
- Przeglądanie szczegółów części

## Technologie

- C#
- ASP.NET Core MVC
- Entity Framework Core
- Bootstrap (UI)
- SQL Server (domyślna baza danych)

## Uruchomienie projektu

1. Sklonuj repozytorium:

`git clone <adres_repozytorium>`

2. Przejdź do katalogu projektu:

`cd WarsztatSamochodowyApp`

3. Przygotuj bazę danych (np. migracje EF Core):

`dotnet ef migrations add db`
`dotnet ef database update`

4. Uruchom aplikację:
`dotnet run`

5. Otwórz przeglądarkę i przejdź pod adres `https://localhost:5001` lub `http://localhost:5000`.

## Struktura projektu

- `Controllers/` — kontrolery MVC
- `Models/` — modele danych
- `Views/` — widoki Razor
- `Data/` — konfiguracja bazy danych i kontekst EF Core

## Jakub Połeć Szymon Niedzielski

Projekt stworzony na potrzeby nauki ASP.NET Core MVC.
