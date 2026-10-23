param([string]$BaseUrl = 'http://localhost:5086')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$script:checks = 0
function Assert-True($Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
    $script:checks++
}
function Read-Api([string]$Path, [int]$Status = 200) {
    $reply = Invoke-WebRequest "$BaseUrl/api/$Path" -SkipHttpErrorCheck -TimeoutSec 15
    Assert-True ($reply.StatusCode -eq $Status) "$Path : oodatud $Status, saadi $($reply.StatusCode): $($reply.Content)"
    Assert-True ($reply.Content -notmatch '(?i)password|passwordHash') "$Path : vastuses on salasõnaandmed"
    if ($Status -eq 200) {
        Assert-True ($reply.Headers['Content-Type'] -match 'application/json') "$Path : vastus peab olema JSON"
        $body = $reply.Content | ConvertFrom-Json
        Assert-True (-not $body.HasErrors) "$Path : OperationResult sisaldab vigu"
        return $body.Value
    }
}
foreach ($feature in 'Users', 'TodoLists', 'TodoItems') {
    $count = if ($feature -eq 'TodoItems') { 245 } else { 35 }
    $page = Read-Api "$feature/list/1/10"
    Assert-True ($page.RowCount -eq $count) "$feature : vale algandmete arv"
    Assert-True ($page.CurrentPage -eq 1 -and $page.PageSize -eq 10) "$feature : vale lehekülje metaandmestik"
    Assert-True ($page.PageCount -eq [Math]::Ceiling($count / 10.0)) "$feature : vale lehekülgede arv"
    Assert-True ($page.Results.Count -eq 10) "$feature : vale kirjete arv lehel"
    $second = Read-Api "$feature/list/2/10"
    Assert-True ($second.Results[0].Id -gt $page.Results[-1].Id) "$feature : lehed kattuvad"
    $last = Read-Api "$feature/list/$($page.PageCount)/10"
    Assert-True ($last.Results.Count -eq 5) "$feature : vale viimase lehe suurus"
    $empty = Read-Api "$feature/list/2147483647/100"
    Assert-True ($empty.Results.Count -eq 0 -and $empty.RowCount -eq $count) "$feature : puuduv leht peab olema tühi"
    $normalized = Read-Api "$feature/list/0/-1"
    Assert-True ($normalized.CurrentPage -eq 1 -and $normalized.PageSize -eq 10) "$feature : vigaste piirväärtuste normaliseerimine"
    $maximum = Read-Api "$feature/list/1/2147483647"
    Assert-True ($maximum.PageSize -eq 100) "$feature : maksimaalne lehe suurus"
    $default = Read-Api "$feature/list"
    Assert-True ($default.Results.Count -eq 10) "$feature : vaikimisi lehekülgjaotus"
    $detail = Read-Api "$feature/get/1"
    Assert-True ($detail.Id -eq 1) "$feature : vale detailkirje"
    foreach ($id in 0, -1, 2147483647) { Read-Api "$feature/get/$id" 404 }
}
$user = Read-Api 'Users/get/1'
$list = Read-Api 'TodoLists/get/1'
$item = Read-Api 'TodoItems/get/1'
Assert-True ($user.TodoListCount -eq $user.TodoLists.Count) 'Kasutaja loendite arv ei klapi'
Assert-True ($list.ItemCount -eq $list.Items.Count) 'Loendi ülesannete arv ei klapi'
Assert-True ($list.CompletedItemCount -eq @($list.Items | Where-Object IsCompleted).Count) 'Valmis ülesannete arv ei klapi'
Assert-True ($list.UserName -eq $user.UserName -and $list.UserId -eq $user.Id) 'Loendi omanik ei klapi'
Assert-True ($item.TodoListTitle -eq $list.Title -and $item.TodoListId -eq $list.Id) 'Ülesande loend ei klapi'
Write-Output "API kontrollid läbitud: $script:checks"
