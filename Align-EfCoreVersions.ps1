# Align-EfCoreVersions.ps1
# Выравнивает версии пакетов Microsoft.EntityFrameworkCore.* во всех .csproj решения
# по максимальной найденной версии.

$ErrorActionPreference = 'Stop'
$root = Get-Location

Write-Host "Корень решения: $root" -ForegroundColor Cyan

# 1. Находим все .csproj (кроме bin/obj)
$projects = Get-ChildItem -Path $root -Recurse -Filter *.csproj |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }

if (-not $projects) {
    Write-Host "Не найдено ни одного .csproj" -ForegroundColor Red
    exit 1
}

Write-Host "`nНайдено проектов: $($projects.Count)" -ForegroundColor Cyan

# 2. Собираем все версии EF Core пакетов
$pattern = 'PackageReference\s+Include="(Microsoft\.EntityFrameworkCore(?:\.[A-Za-z]+)*)"\s+Version="([^"]+)"'
$allVersions = @{}

foreach ($proj in $projects) {
    $content = Get-Content -Raw -Path $proj.FullName
    $matches = [regex]::Matches($content, $pattern)
    foreach ($m in $matches) {
        $pkg = $m.Groups[1].Value
        $ver = $m.Groups[2].Value
        if (-not $allVersions.ContainsKey($pkg)) {
            $allVersions[$pkg] = @()
        }
        $allVersions[$pkg] += [PSCustomObject]@{
            Project = $proj.FullName
            Version = $ver
        }
    }
}

if ($allVersions.Count -eq 0) {
    Write-Host "Пакеты Microsoft.EntityFrameworkCore.* не найдены в .csproj" -ForegroundColor Yellow
    exit 0
}

# 3. Определяем целевую версию для каждого пакета = максимальная
$targetVersions = @{}
foreach ($pkg in $allVersions.Keys) {
    $versions = $allVersions[$pkg] | ForEach-Object { [version]$_.Version }
    $max = ($versions | Sort-Object -Descending | Select-Object -First 1).ToString()
    $targetVersions[$pkg] = $max
}

Write-Host "`nЦелевые версии (максимальные по решению):" -ForegroundColor Green
$targetVersions.GetEnumerator() | Sort-Object Name | ForEach-Object {
    Write-Host ("  {0} -> {1}" -f $_.Key, $_.Value)
}

# 4. Показываем, что будет изменено
Write-Host "`nФайлы, подлежащие изменению:" -ForegroundColor Yellow
$changedProjects = @{}
foreach ($pkg in $allVersions.Keys) {
    $target = $targetVersions[$pkg]
    foreach ($entry in $allVersions[$pkg]) {
        if ($entry.Version -ne $target) {
            $changedProjects[$entry.Project] = $true
            Write-Host ("  {0}" -f $entry.Project) -ForegroundColor Yellow
        }
    }
}

if ($changedProjects.Count -eq 0) {
    Write-Host "`nВсе версии уже выровнены. Ничего делать не нужно." -ForegroundColor Green
    exit 0
}

# 5. Спрашиваем подтверждение
$confirm = Read-Host "`nВнести изменения? (y/n)"
if ($confirm -ne 'y') {
    Write-Host "Отменено." -ForegroundColor Red
    exit 0
}

# 6. Правим файлы (с бэкапом .bak)
foreach ($projPath in $changedProjects.Keys) {
    $content = Get-Content -Raw -Path $projPath
    $original = $content

    foreach ($pkg in $targetVersions.Keys) {
        $target = $targetVersions[$pkg]
        # Заменяем версию только у этого пакета
        $regex = '(<PackageReference\s+Include="' + [regex]::Escape($pkg) + '"\s+Version=")([^"]+)(")'
        $content = [regex]::Replace($content, $regex, { param($m) $m.Groups[1].Value + $target + $m.Groups[3].Value })
    }

    if ($content -ne $original) {
        Copy-Item -Path $projPath -Destination "$projPath.bak" -Force
        Set-Content -Path $projPath -Value $content -NoNewline
        Write-Host "  Обновлён: $projPath" -ForegroundColor Green
    }
}

Write-Host "`nГотово. Бэкапы сохранены рядом с .csproj (*.csproj.bak)." -ForegroundColor Cyan
Write-Host "Проверь изменения: git diff" -ForegroundColor Cyan