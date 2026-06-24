param(
    [ValidateSet("Auto", "A", "B")]
    [string]$Slot = "Auto"
)

$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectFile = Join-Path $projectRoot "CPortTerminal.csproj"
$stagingDir = Join-Path $projectRoot "bin\Release\publish-staging"
$slotsRoot = Join-Path $projectRoot "bin\Release\test-slots"
$slotExeNames = @{
    A = "CPortTerminal-A.exe"
    B = "CPortTerminal-B.exe"
}

function Test-SlotProcess {
    param([ValidateSet("A", "B")][string]$SlotName)

    $processName = [System.IO.Path]::GetFileNameWithoutExtension($slotExeNames[$SlotName])
    return @(Get-Process -Name $processName -ErrorAction SilentlyContinue).Count -gt 0
}

function Get-SlotDirectory {
    param([ValidateSet("A", "B")][string]$SlotName)

    return Join-Path $slotsRoot $SlotName
}

function Get-SlotExePath {
    param([ValidateSet("A", "B")][string]$SlotName)

    return Join-Path (Get-SlotDirectory $SlotName) $slotExeNames[$SlotName]
}

function Select-AutoSlot {
    $aRunning = Test-SlotProcess "A"
    $bRunning = Test-SlotProcess "B"

    if ($aRunning -and $bRunning) {
        throw "Both CPortTerminal-A.exe and CPortTerminal-B.exe are running. Close one before publishing."
    }

    if ($aRunning) {
        return "B"
    }

    if ($bRunning) {
        return "A"
    }

    $aExe = Get-SlotExePath "A"
    $bExe = Get-SlotExePath "B"

    if (-not (Test-Path -LiteralPath $aExe)) {
        return "A"
    }

    if (-not (Test-Path -LiteralPath $bExe)) {
        return "B"
    }

    $aTime = (Get-Item -LiteralPath $aExe).LastWriteTimeUtc
    $bTime = (Get-Item -LiteralPath $bExe).LastWriteTimeUtc

    if ($aTime -ge $bTime) {
        return "B"
    }

    return "A"
}

if ($Slot -eq "Auto") {
    $Slot = Select-AutoSlot
}

if (Test-SlotProcess $Slot) {
    throw "$($slotExeNames[$Slot]) is running. Close it or publish to the other slot."
}

$slotDir = Get-SlotDirectory $Slot
$slotExe = Get-SlotExePath $Slot

New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null
New-Item -ItemType Directory -Path $slotDir -Force | Out-Null

dotnet publish $projectFile -c Release -o $stagingDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Get-ChildItem -LiteralPath $stagingDir -File |
    Where-Object { $_.Name -ne "CPortTerminal.exe" } |
    Copy-Item -Destination $slotDir -Force

Copy-Item -LiteralPath (Join-Path $stagingDir "CPortTerminal.exe") -Destination $slotExe -Force

$assetSource = Join-Path $stagingDir "Assets"
if (Test-Path -LiteralPath $assetSource) {
    Copy-Item -LiteralPath $assetSource -Destination $slotDir -Recurse -Force
}

Write-Output "Slot=$Slot"
Write-Output "Exe=$slotExe"
