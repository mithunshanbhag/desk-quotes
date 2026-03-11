[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

Push-Location $workspaceRoot

try {
    if (-not (Get-Command jb -ErrorAction SilentlyContinue)) {
        throw 'The jb command was not found on PATH. Install or expose JetBrains.ReSharper.GlobalTools before using this hook.'
    }

    $solutions = @(Get-ChildItem -Path $workspaceRoot -Filter *.slnx -File | Sort-Object -Property FullName)

    if ($solutions.Count -eq 0) {
        Write-Host 'No .slnx files found. Skipping ReSharper cleanup.'
        exit 0
    }

    Write-Host 'Running ReSharper CleanupCode for solution files in the workspace.'
    Write-Host 'CleanupCode resolves best after a successful build of the target solution.'

    foreach ($solution in $solutions) {
        Write-Host ("jb cleanupcode {0}" -f $solution.Name)
        & jb cleanupcode $solution.FullName

        if ($LASTEXITCODE -ne 0) {
            throw ("jb cleanupcode failed for {0} with exit code {1}." -f $solution.FullName, $LASTEXITCODE)
        }
    }
}
finally {
    Pop-Location
}