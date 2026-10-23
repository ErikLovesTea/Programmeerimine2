param([int]$Port = 5097)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projectRoot = Split-Path $PSScriptRoot -Parent
$dll = Join-Path $projectRoot 'KooliProjekt.WebAPI/bin/Debug/net10.0/KooliProjekt.WebAPI.dll'
if (-not (Test-Path $dll)) { throw 'Käivita esmalt dotnet build KooliProjekt.sln' }
# Test puudutab ainult selle käivituse jaoks loodud juhusliku nimega andmebaasi.
$database = 'KooliProjekt_Verify_' + [Guid]::NewGuid().ToString('N')
$script:appProcess = $null
function Stop-TestApp {
    if ($null -ne $script:appProcess -and -not $script:appProcess.HasExited) {
        $script:appProcess.Kill($true)
        $script:appProcess.WaitForExit()
    }
}
function Start-TestApp([string]$Stage) {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new('dotnet')
    $startInfo.WorkingDirectory = Join-Path $projectRoot 'KooliProjekt.WebAPI'
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $startInfo.ArgumentList.Add($dll)
    $startInfo.Environment['ASPNETCORE_URLS'] = "http://localhost:$Port"
    $startInfo.Environment['ASPNETCORE_ENVIRONMENT'] = 'Development'
    $startInfo.Environment['ConnectionStrings__DefaultConnection'] = "Server=(localdb)\MSSQLLocalDB;Database=$database;Trusted_Connection=True"
    $startInfo.Environment['Logging__LogLevel__Default'] = 'Warning'
    $script:appProcess = [System.Diagnostics.Process]::Start($startInfo)
    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        if ($script:appProcess.HasExited) { throw "API lõpetas töö: $Stage" }
        try {
            $response = Invoke-WebRequest "http://localhost:$Port/api/Users/list" -TimeoutSec 1
            if ($response.StatusCode -eq 200) { $ready = $true; break }
        } catch { Start-Sleep -Milliseconds 500 }
    }
    if (-not $ready) { throw "API ei käivitunud: $Stage" }
    & (Join-Path $PSScriptRoot 'Verify-Api.ps1') -BaseUrl "http://localhost:$Port"
    $swagger = Invoke-RestMethod "http://localhost:$Port/swagger/v1/swagger.json"
    if (@($swagger.paths.PSObject.Properties).Count -lt 9) { throw 'Swaggeris puuduvad API teekonnad' }
    Write-Output "$Stage : korras"
}
function Remove-TestDatabase {
    if ($database -notmatch '^KooliProjekt_Verify_[a-f0-9]{32}$') { throw 'Sobimatu testandmebaasi nimi' }
    & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -b -Q "IF DB_ID(N'$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END"
    if ($LASTEXITCODE -ne 0) { throw 'Testandmebaasi eemaldamine ebaõnnestus' }
}
try {
    Start-TestApp 'Uue andmebaasi migratsioon ja algandmed'
    Stop-TestApp
    Start-TestApp 'Korduskäivitus ei dubleeri andmeid'
    Stop-TestApp
    Remove-TestDatabase
    Start-TestApp 'Kustutatud andmebaasi automaatne taasloomine'
    Write-Output 'Kõik andmebaasi ja API kontrollid läbitud.'
} finally {
    Stop-TestApp
    Remove-TestDatabase
}
