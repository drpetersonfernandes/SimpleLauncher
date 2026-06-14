param(
    [string]$Filter = "",
    [string]$Verbosity = "normal",
    [switch]$NoBuild,
    [switch]$FailFast
)

$ErrorActionPreference = "Stop"

$resultsDir = Join-Path $PSScriptRoot "TestResults"
if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force -ErrorAction SilentlyContinue }
New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null

$trxFile = Join-Path $resultsDir "results.trx"

$testProject = Join-Path (Join-Path $PSScriptRoot "SimpleLauncher.Tests") "SimpleLauncher.Tests.csproj"
if (-not (Test-Path $testProject)) {
    $testProject = Join-Path $PSScriptRoot "SimpleLauncher.Tests.csproj"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "       SimpleLauncher Test Runner       " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$dotnetArgs = @("test", $testProject, "--logger", "trx;LogFileName=results.trx", "--results-directory", $resultsDir, "--verbosity", $Verbosity)
if ($NoBuild) { $dotnetArgs += "--no-build" }
if ($Filter) { $dotnetArgs += "--filter", $Filter }

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$oldEA = $ErrorActionPreference
$ErrorActionPreference = "Continue"
$dotnetOutput = & dotnet $dotnetArgs 2>&1
$exitCode = $LASTEXITCODE
$ErrorActionPreference = $oldEA
$sw.Stop()

if (-not (Test-Path $trxFile)) {
    Write-Host "`nERROR: TRX file not found at $trxFile" -ForegroundColor Red
    exit 1
}

[xml]$trx = Get-Content $trxFile
$ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
$ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")

$results = $trx.SelectNodes("//t:UnitTestResult", $ns)

$passed = @()
$failed = @()
$skipped = @()

foreach ($r in $results) {
    $name = $r.testName
    $outcome = $r.outcome
    $duration = $r.duration

    $ts = [TimeSpan]::Zero
    if ($duration -match "(\d+)\.(\d+):(\d+):(\d+)\.(\d+)") {
        $ts = [TimeSpan]::FromDays([int]$Matches[1]) + [TimeSpan]::FromHours([int]$Matches[2]) + [TimeSpan]::FromMinutes([int]$Matches[3]) + [TimeSpan]::FromSeconds([int]$Matches[4]) + [TimeSpan]::FromMilliseconds([int]$Matches[5].Substring(0,3))
    } elseif ($duration -match "(\d+):(\d+):(\d+)\.(\d+)") {
        $ts = [TimeSpan]::FromHours([int]$Matches[1]) + [TimeSpan]::FromMinutes([int]$Matches[2]) + [TimeSpan]::FromSeconds([int]$Matches[3]) + [TimeSpan]::FromMilliseconds([int]$Matches[4].Substring(0,3))
    }

    if ($ts.TotalSeconds -lt 1) { $elapsedStr = "$([int]$ts.TotalMilliseconds)ms" }
    else { $elapsedStr = "{0:F1}s" -f $ts.TotalSeconds }

    $entry = @{
        Name     = $name
        Duration = $elapsedStr
    }

    switch ($outcome) {
        "Passed" {
            $passed += $entry
            Write-Host "  >>> PASSED <<< [$elapsedStr] $name" -ForegroundColor Green
        }
        "Failed" {
            $errorNode = $r.SelectSingleNode("t:Output/t:ErrorInfo", $ns)
            $errorMsg = ""
            $stackTrace = ""
            if ($errorNode) {
                $msgNode = $errorNode.SelectSingleNode("t:Message", $ns)
                $stackNode = $errorNode.SelectSingleNode("t:StackTrace", $ns)
                if ($msgNode) { $errorMsg = $msgNode.InnerText }
                if ($stackNode) { $stackTrace = $stackNode.InnerText }
            }
            $entry.Error = $errorMsg
            $entry.StackTrace = $stackTrace
            $failed += $entry
            Write-Host "  >>> FAILED <<< [$elapsedStr] $name" -ForegroundColor Red
            if ($errorMsg) {
                $firstLine = ($errorMsg -split "`n")[0].Trim()
                Write-Host "           Error: $firstLine" -ForegroundColor DarkRed
            }
        }
        "NotExecuted" {
            $skipped += $entry
            Write-Host "  >>> SKIPPED <<< [$elapsedStr] $name" -ForegroundColor Yellow
        }
        default {
            $passed += $entry
            Write-Host "  >>> PASSED <<< [$elapsedStr] $name" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "           TEST RESULTS SUMMARY         " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Total:    $($results.Count)" -ForegroundColor White
Write-Host "  Passed:   $($passed.Count)" -ForegroundColor Green
Write-Host "  Failed:   $($failed.Count)" -ForegroundColor $(if ($failed.Count -gt 0) { "Red" } else { "Green" })
Write-Host "  Skipped:  $($skipped.Count)" -ForegroundColor $(if ($skipped.Count -gt 0) { "Yellow" } else { "White" })
Write-Host "  Duration: $($sw.Elapsed.ToString('mm\:ss'))" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

if ($failed.Count -gt 0) {
    Write-Host ""
    Write-Host "FAILED TESTS DETAIL:" -ForegroundColor Red
    Write-Host "--------------------" -ForegroundColor Red
    $i = 1
    foreach ($f in $failed) {
        Write-Host ""
        Write-Host "  $i. $($f.Name)" -ForegroundColor Red
        Write-Host "     Duration: $($f.Duration)" -ForegroundColor DarkGray
        if ($f.Error) {
            $lines = $f.Error -split "`n" | Where-Object { $_.Trim() -ne "" }
            foreach ($line in $lines) {
                Write-Host "     $($line.Trim())" -ForegroundColor DarkRed
            }
        }
        $i++
    }
    Write-Host ""
}

if ($skipped.Count -gt 0) {
    Write-Host ""
    Write-Host "SKIPPED TESTS:" -ForegroundColor Yellow
    Write-Host "--------------" -ForegroundColor Yellow
    foreach ($s in $skipped) {
        Write-Host "  - $($s.Name)" -ForegroundColor Yellow
    }
    Write-Host ""
}

Write-Host "TRX report: $trxFile" -ForegroundColor DarkGray
Write-Host ""

if ($failed.Count -gt 0) {
    if ($FailFast) {
        Write-Host "FAIL-FAST: Stopping on first failure." -ForegroundColor Red
    }
    exit 1
}

exit 0
