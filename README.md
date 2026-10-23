# KooliProjekt

Programmeerimine 2 õppeprojekt: ülesannete haldamise Web API, mille aluseks on õpetaja 02.10, 08.10, 16.10 ja 23.10 näited. Lahendus katab andmemudeli, migratsiooni, algandmed ning kõigi tabelite Get/List-päringud koos DTO-de, OperationResulti ja lehekülgjaotusega.

## Käivitamine

Vajalikud on .NET 10 SDK ning Windowsis SQL Server Express LocalDB. Automaatkontrollid kasutavad lisaks PowerShell 7 ja sqlcmd-d.

```powershell
dotnet restore KooliProjekt.sln
dotnet build KooliProjekt.sln
dotnet run --project KooliProjekt.WebAPI --launch-profile http
```

Swagger: http://localhost:5086/swagger

Visual Studios ava `KooliProjekt.sln` ja vali käivitusprojektiks **KooliProjekt.WebAPI** (paremklõps → Set as Startup Project). Kaasas on ka selle projekti käivitav `KooliProjekt.slnLaunch` profiil. Application on klassiteek.

Esimesel käivitamisel rakendab `Program.cs` migratsioonid ja käivitab `SeedData.Generate`. Ühendus on `KooliProjekt.WebAPI/appsettings.json` failis:

```text
Server=(localdb)\MSSQLLocalDB;Database=KooliProjekt_Opilane;Trusted_Connection=True
```

Algandmed: **35 kasutajat, 35 ülesannete loendit ja 245 ülesannet**. Kasutajanimed ja koolitööde kirjeldused on sünteetilised. Seosed luuakse andmebaasi tegelike ID-de kaudu. Korduskäivitus ei dubleeri olemasolevaid algandmeid; puuduvad näidiskirjed lisatakse tagasi. Genereerimine on ühes tehingus ja seda saab välja lülitada seadistusega `SeedData:Enabled=false`. Kasutajatel on juhuslike salasõnade PBKDF2-räsid; sisselogimine ei kuulu selle ülesande skoopi.

## API

Kõik kontrollerid pärivad `ApiControllerBase`-ist ja saadavad päringud MediatR-i kaudu.

| Andmed | Üks kirje | Lehekülg |
| --- | --- | --- |
| Kasutajad | GET /api/Users/get/1 | GET /api/Users/list/1/10 |
| Loendid | GET /api/TodoLists/get/1 | GET /api/TodoLists/list/1/10 |
| Ülesanded | GET /api/TodoItems/get/1 | GET /api/TodoItems/list/1/10 |

`/api/{kontroller}/list` kasutab vaikimisi esimest lehekülge ja lehe suurust 10. Päringuparameetrid töötavad samuti: `/api/Users/list?page=2&pageSize=5`.

Kõigi Get-päringute vastusetüüp on `OperationResult<DetailDto>` ning List-päringutel `OperationResult<PagedResult<ListItemDto>>`. JSON tagastatakse objektina `application/json` vormingus.

```json
{
  "value": {
    "results": [{ "id": 1, "userName": "mari.tamm", "todoListCount": 1 }],
    "currentPage": 1,
    "pageCount": 35,
    "pageSize": 1,
    "rowCount": 35
  },
  "propertyErrors": null,
  "errors": null,
  "hasErrors": false
}
```

- `get/0`, negatiivne ID ja puuduv positiivne ID annavad **404**. Baaskontroller käsitleb ka null-vastust ja null-väärtust.
- OperationResulti vead annavad **400**; tehnilised vead logitakse serveris, kliendile läheb üldine veateade.
- List tagastab kirjete puudumisel **200** koos tühja `results` massiiviga.
- Lehekülg alla ühe normaliseeritakse üheks; lehe suurus kuni nulliks normaliseeritakse kümneks; ülempiir on 100. Väga suur leheküljenumber tagastab tühja lehe ilma täisarvu ületäitumiseta.
- Loendid sorditakse ID kasvamise järgi enne Skip/Take rakendamist. Päringud kasutavad `AsNoTracking`-ut, DTO-projektsioone ja tühistamist.

## Miks just need DTO-d?

| DTO | Lõppkasutajale vajalik info |
| --- | --- |
| UserListItemDto | Kasutajanimi ja loendite arv aitavad kasutajaid leida ja võrrelda. |
| UserDto | Lisaks kasutaja loendid koos pealkirjade ja edenemisega, et avada õige loend. |
| TodoListItemDto | Pealkiri, omaniku nimi ja ID ning tehtud/kõigi ülesannete arv võimaldavad kuvada ülevaadet ja edenemist. |
| TodoListDto | Lisaks järjestatud ülesanded koos lõpetamise olekuga, et näha loendi sisu. |
| TodoItemDto | Ülesande pealkiri, lõpetamise olek, loendi nimi/ID ja omaniku nimi annavad ülesandele konteksti. |

Detail- ja loendivaated kasutavad eri DTO-sid seal, kus detailvaade vajab rohkem andmeid. Ülesandele piisab samast DTO-st mõlemas vaates. Parooliräsi ei kuulu ühtegi DTO-sse. EF-i navigeerimisobjekte API kaudu ei väljastata, seega ei teki tsüklilist JSON-i.

## Migratsioonid ja andmete kontroll

Kaasas on õpetaja mudeliga ühilduv `InitialCreate` migratsioon ja mudeli snapshot. See loob tabelid Users, TodoLists ja TodoItems, kasutajanime unikaalse indeksi ning välisvõtmed. Rakenduse käivitamisel rakendatakse migratsioon automaatselt. Sama muutmata mudeli jaoks uut identset migratsiooni vaja ei ole.

Visual Studio Package Manager Console'is:

```powershell
Update-Database -Project KooliProjekt.Application -StartupProject KooliProjekt.WebAPI
# Alles pärast andmemudeli muutmist:
Add-Migration KirjeldavMuudatuseNimi -Project KooliProjekt.Application -StartupProject KooliProjekt.WebAPI
Update-Database -Project KooliProjekt.Application -StartupProject KooliProjekt.WebAPI
```

SQL Server Object Exploreris ühendu `(localdb)\MSSQLLocalDB` serveriga, tee Databases all Refresh ja ava `KooliProjekt_Opilane`. Tabelite kontrollpäring:

```sql
SELECT 'Users' AS Tabel, COUNT(*) AS Ridu FROM Users
UNION ALL SELECT 'TodoLists', COUNT(*) FROM TodoLists
UNION ALL SELECT 'TodoItems', COUNT(*) FROM TodoItems;
```

Breakpointide harjutus: pane katkestuspunkt kontrolleri `if (id <= 0)` juurde ja baaskontrolleri `if (result.Value == null)` juurde. Käivita siluriga ning küsi vastavalt `get/0` ja `get/2147483647`. Olemasoleva ID jaoks kasuta `get/1`.

## Automaatsed kontrollid

Käivitatud API kontrollimiseks (164 väidet):

```powershell
pwsh -NoProfile -File tests/Verify-Api.ps1
```

Andmebaasi täieliku elutsükli kontrollimiseks pärast Debug buildi:

```powershell
pwsh -NoProfile -File tests/Verify-Database.ps1
```

Teine skript loob eraldi juhusliku nimega testandmebaasi ja kasutab porti 5097. See kontrollib uue andmebaasi migratsiooni/algandmeid, korduskäivitust ilma duplikaatideta ja kustutamise järel automaatset taasloomist. Igas etapis kontrollitakse kogu API-t ja Swaggerit. Lõpus suletakse testprotsess ja eemaldatakse ainult skripti loodud testandmebaas. Rakenduse põhiandmebaasi see ei kustuta.

Kontrollitud 23.10.2026: build õnnestus 0 vea ja 0 hoiatusega; API 164 kontrolli läbitud kõigis kolmes andmebaasi etapis (kokku 492), samuti Swaggeri kontrollid.

## C# ja CQRS

CQRS eraldab andmete lugemise päringuteks (Query) ja muutmise käskudeks (Command). Selles töös on realiseeritud lugemise pool: kontroller loob Query, MediatR leiab vastava Handler-i, Handler teeb EF Core'i päringu ja projitseerib tulemuse DTO-ks. OperationResult koondab tulemuse ning vead, PagedResult lisab loendi leheküljeinfo. Eraldi lugemisandmebaasi ega sündmuste salvestamist selle ülesande jaoks vaja pole.

Allikad: [Microsofti CQRS-mustri kirjeldus](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) ja [C# juhend](https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/).
