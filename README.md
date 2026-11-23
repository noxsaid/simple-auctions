# Simple Auctions

En liten startpunkt för kursprojektet i **Test, integration, leverans**. C#/.NET 10 Minimal API, MySQL och explicita SQL-frågor med MySqlConnector. Strukturen följer [MovieAPI](https://github.com/no92one/MovieAPI) och kursguiderna om REST och sessionsinloggning.

## Kom igång

Du behöver .NET 10 SDK och MySQL 8.0.16 eller senare (exempelvis MySQL 8.4).

1. Klona repositoryt och gå till projektmappen.
2. Kör `Database/script.sql` i din SQL-klient. Skriptet skapar databas och tabeller utan att radera befintlig data.
3. Skapa ett separat databaskonto. Kör med ett administratörskonto och välj ett eget lösenord:

```sql
CREATE USER 'auctionuser'@'localhost' IDENTIFIED BY 'byt-till-eget-losenord';
GRANT SELECT, INSERT, UPDATE, DELETE ON simple_auctions.* TO 'auctionuser'@'localhost';
```

Anpassa host om MySQL körs i exempelvis Docker.

4. Konfigurera lokal anslutning från projektmappen:

```bash
dotnet user-secrets set "ConnectionStrings:AuctionDatabase" "Server=localhost;Port=3306;Database=simple_auctions;User ID=auctionuser;Password=byt-till-eget-losenord;"
dotnet run
```

API:et kör på `http://localhost:5000` med den medföljande utvecklingsprofilen. Konfiguration kan också ges med miljövariabeln `ConnectionStrings__AuctionDatabase`. Inga riktiga anslutningsuppgifter ska checkas in.

## Struktur

| Del | Ansvar |
| --- | --- |
| `Program.cs` | Konfiguration, DI, session och registrering av endpoints |
| `Models/` | User, Auction och Bid som records |
| `DTOs/` | Data in till API:et och användarsvar utan lösenordshash |
| `Endpoints/` | HTTP, session, grundvalidering och svar |
| `Repositories/` | Parameteriserad SQL och mappning från MySQL |
| `Rules/BidRules.cs` | Budregler som kan testas utan databas |
| `Database/script.sql` | Databas och tre tabeller |

En budläggning går från endpoint till BidRepository, som kontrollerar att auktionen finns, anropar `BidRules.Validate(amount)` och sparar budet. Budreglerna är avsiktligt ofullständiga: studenterna ska undersöka och vidareutveckla dem.

## Databas och regler

| Tabell | Kolumner |
| --- | --- |
| users | id, email (unik), password_hash |
| auctions | id, name, end_time, seller_id |
| bids | id, amount, time, buyer_id, auction_id |

- Auktioner startar direkt och måste ha namn och en framtida sluttid.
- Budregel att beloppet får ha högst två decimaler.
- Inloggning krävs för att skapa auktioner och bud.
- Alla tider lagras i UTC. Skicka `endTime` som ISO 8601 med `Z` eller explicit offset.
- Säljare och köpare tas från sessionen. Budets tid sätts av servern.
- Auktionens status härleds från tiden.
- Det finns inga startpriser, reservationspriser, dolda bud, adminroller, ändrings-/raderingsfunktioner eller alternativa auktionstyper.

`GET /api/auctions/{id}` visar även `ended`, `currentPrice`, `winnerId` och `bids`. Pris är `null` före första budet och vinnare är `null` före avslut eller om bud saknas. Dessa värden lagras inte separat.

## Endpoints

| Metod | URL | Funktion |
| --- | --- | --- |
| POST | /api/users | Registrera användare |
| GET | /api/users/{id} | Visa egen användare (inloggning krävs) |
| POST | /api/auth/login | Logga in |
| GET | /api/auth/login | Kontrollera inloggning |
| DELETE | /api/auth/login | Logga ut |
| GET | /api/auctions | Lista auktioner |
| GET | /api/auctions/{id} | Visa auktion, bud och resultat |
| POST | /api/auctions | Skapa auktion |
| GET | /api/auctions/{id}/bids | Visa öppna bud |
| POST | /api/auctions/{id}/bids | Lägg bud |

Exempel på request bodies:

```json
{ "email": "seller@example.test", "password": "DemoPassword123!" }
```

Samma format används för registrering och inloggning.

```json
{ "name": "En gammal synt", "endTime": "2030-01-01T12:00:00Z" }
```

## Avgränsning

Sessionsinloggningen följer kursens pedagogiska exempel med `HttpContext.Session`. Lösenord hashats med ASP.NET Cores `PasswordHasher`, och hash returneras aldrig till klienten. Sessioner finns i processens minne, försvinner vid omstart och delas inte mellan instanser. Produktionsläge kräver HTTPS för cookien. Produktionsautentisering, sessionsrotation, CSRF-skydd och inloggningsbegränsning behöver behandlas före verklig användning. 

Vidareutveckla startpunkten med testning, dokumentation och små funktionella steg enligt projektuppgiften.
