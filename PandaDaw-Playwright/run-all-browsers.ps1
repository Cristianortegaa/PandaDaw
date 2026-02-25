<#
.SYNOPSIS
    Ejecuta los tests de Playwright E2E en los tres navegadores: Chromium, Firefox y WebKit.
    Los resultados se guardan en la carpeta results/ separados por navegador.

.DESCRIPTION
    Este script ejecuta dotnet test tres veces, una por cada navegador,
    usando los archivos .runsettings correspondientes.
    Videos, screenshots y trazas se generan en results/.

.EXAMPLE
    .\run-all-browsers.ps1
#>

$ErrorActionPreference = "Continue"
$projectDir = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PandaDaw - Playwright E2E Tests" -ForegroundColor Cyan
Write-Host "  Ejecutando en TODOS los navegadores" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$browsers = @("chromium", "firefox", "webkit")
$resultados = @{}

foreach ($browser in $browsers) {
    Write-Host ""
    Write-Host "────────────────────────────────────────" -ForegroundColor Yellow
    Write-Host "  Ejecutando tests en: $($browser.ToUpper())" -ForegroundColor Yellow
    Write-Host "────────────────────────────────────────" -ForegroundColor Yellow

    $settingsFile = Join-Path $projectDir "$browser.runsettings"
    $resultDir = Join-Path $projectDir "results" $browser

    # Crear directorio de resultados por navegador
    New-Item -ItemType Directory -Force -Path $resultDir | Out-Null

    # Ejecutar tests
    $startTime = Get-Date
    dotnet test $projectDir `
        --settings:$settingsFile `
        --results-directory:$resultDir `
        --logger "trx;LogFileName=$browser-results.trx" `
        --logger "console;verbosity=normal" `
        2>&1 | Tee-Object -Variable output

    $endTime = Get-Date
    $duration = $endTime - $startTime
    $exitCode = $LASTEXITCODE

    $resultados[$browser] = @{
        ExitCode = $exitCode
        Duration = $duration
        Status   = if ($exitCode -eq 0) { "PASSED" } else { "FAILED" }
    }

    if ($exitCode -eq 0) {
        Write-Host "  ✅ $($browser.ToUpper()): PASSED ($([math]::Round($duration.TotalSeconds, 1))s)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $($browser.ToUpper()): FAILED ($([math]::Round($duration.TotalSeconds, 1))s)" -ForegroundColor Red
    }
}

# ── Resumen final ──
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DE RESULTADOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

foreach ($browser in $browsers) {
    $r = $resultados[$browser]
    $color = if ($r.Status -eq "PASSED") { "Green" } else { "Red" }
    $icon = if ($r.Status -eq "PASSED") { "✅" } else { "❌" }
    Write-Host "  $icon $($browser.ToUpper().PadRight(10)) $($r.Status.PadRight(8)) ($([math]::Round($r.Duration.TotalSeconds, 1))s)" -ForegroundColor $color
}

Write-Host ""
Write-Host "  Resultados guardados en: results/" -ForegroundColor Gray
Write-Host "  - Screenshots: results/screenshots/" -ForegroundColor Gray
Write-Host "  - Videos:      results/videos/" -ForegroundColor Gray
Write-Host "  - Trazas:      results/traces/" -ForegroundColor Gray
Write-Host ""

# Devolver código de error si alguno falló
$anyFailed = $resultados.Values | Where-Object { $_.ExitCode -ne 0 }
if ($anyFailed) {
    Write-Host "  ⚠️  Algunos tests fallaron. Revisa las trazas para más detalle." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "  🎉 ¡Todos los navegadores pasaron!" -ForegroundColor Green
    exit 0
}
