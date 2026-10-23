# KooliProjekt

Ülesannete haldamise Web API, mis on tehtud Programmeerimine 2 õppeaine raames õpetaja näidete põhjal.

Rakendus kasutab C#-i, .NET 10, ASP.NET Core'i, Entity Framework Core'i, SQL Serverit ja MediatR-i.

## Käivitamine

Vaja on .NET 10 SDK-d ja SQL Server Express LocalDB-d.

Ava Visual Studios `KooliProjekt.sln` ja vali käivitusprojektiks `KooliProjekt.WebAPI`. Terminalist käivitamiseks:

```powershell
dotnet restore KooliProjekt.sln
dotnet build KooliProjekt.sln
dotnet run --project KooliProjekt.WebAPI --launch-profile http
```

Swagger avaneb aadressil [http://localhost:5086/swagger](http://localhost:5086/swagger).

Ühendus on seadistatud failis [appsettings.json](KooliProjekt.WebAPI/appsettings.json). Vaikimisi kasutatakse serverit `(localdb)\MSSQLLocalDB` ja andmebaasi `KooliProjekt_Opilane`.

Käivitamisel rakendatakse migratsioonid ning lisatakse 35 kasutajat, 35 ülesannete loendit ja 245 ülesannet. Korduskäivitus ei dubleeri algandmeid. Andmete lisamise saab välja lülitada seadistusega `SeedData:Enabled=false`.

## API

| Andmed | Ühe kirje päring | Lehekülje päring |
| --- | --- | --- |
| Kasutajad | `GET /api/Users/get/1` | `GET /api/Users/list/1/10` |
| Loendid | `GET /api/TodoLists/get/1` | `GET /api/TodoLists/list/1/10` |
| Ülesanded | `GET /api/TodoItems/get/1` | `GET /api/TodoItems/list/1/10` |

List-päringu viimased kaks arvu on lehekülje number ja suurus. `/list` kasutab vaikimisi väärtusi 1 ja 10; toetatud on ka `/list?page=2&pageSize=5`. Lehe suuruse ülempiir on 100.

Get tagastab `OperationResult<Dto>` ja List `OperationResult<PagedResult<Dto>>`. Puuduv kirje ning ID, mis on nullist väiksem või sellega võrdne, annavad 404. Tühi loend tagastatakse 200 vastusena koos tühja `results` massiiviga.

Näidispäringud on failis [KooliProjekt.WebAPI.http](KooliProjekt.WebAPI/KooliProjekt.WebAPI.http).

## Ülesehitus

- `KooliProjekt.Application/Data/` — andmemudel, DbContext, algandmed ja lehekülgjaotus.
- `KooliProjekt.Application/Features/` — Get/List query'd ja nende handlerid.
- `KooliProjekt.Application/Dto/` — API vastuste andmekujud.
- `KooliProjekt.Application/Infrastructure/` — OperationResult ja PagedResult.
- `KooliProjekt.Application/Migrations/` — andmebaasi migratsioonid.
- `KooliProjekt.WebAPI/Controllers/` — HTTP päringute vastuvõtmine ja MediatR-i kasutamine.

Päringu teekond: **Controller → Query → Handler → DbContext → DTO → OperationResult → HTTP-vastus**.

DTO-d eraldavad andmebaasi mudeli API vastusest. Kasutaja detail sisaldab tema loendeid ning loendi detail selle ülesandeid. Loendite ülevaates näeb omanikku ja lõpetatud ülesannete arvu. Parooliräsi ei kuulu ühtegi DTO-sse.

## Migratsioonid

Esimene migratsioon on projektis olemas ja rakendatakse automaatselt. Kui andmemudelit muudad, lisa Visual Studio Package Manager Console'is uus migratsioon:

```powershell
Add-Migration MuudatuseNimi -Project KooliProjekt.Application -StartupProject KooliProjekt.WebAPI
Update-Database -Project KooliProjekt.Application -StartupProject KooliProjekt.WebAPI
```

SQL Server Object Exploreris saab andmeid vaadata serveri `(localdb)\MSSQLLocalDB` andmebaasist `KooliProjekt_Opilane`.

## Kontrollimine

Käivitatud API kontrollimiseks:

```powershell
pwsh -NoProfile -File tests/Verify-Api.ps1
```

Andmebaasi loomise, korduskäivituse ja kustutamise järel taasloomise kontrollimiseks pärast Debug buildi:

```powershell
pwsh -NoProfile -File tests/Verify-Database.ps1
```

Skriptid vajavad PowerShell 7; andmebaasi kontroll vajab ka `sqlcmd`-d. Teine skript loob eraldi testandmebaasi ning eemaldab selle lõpus. Rakenduse põhiandmebaasi see ei kustuta.

Siluriga katsetamiseks pane katkestuspunkt kontrolleri ID kontrolli ja baaskontrolleri `result.Value == null` kontrolli juurde. Proovi päringuid `get/0`, `get/2147483647` ning `get/1`.
