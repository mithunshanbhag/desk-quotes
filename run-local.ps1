[CmdletBinding()]
param(
    [ValidateSet("app", "tests", "unit-tests", "e2e-tests")]
    [string]$target = "app"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = (Resolve-Path $PSScriptRoot).Path

function Invoke-DotNetCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw ("dotnet {0} failed with exit code {1}." -f ($Arguments -join ' '), $LASTEXITCODE)
    }
}

Push-Location $workspaceRoot

try {
    switch ($target) {
        "app" {
            Push-Location ".\src\DeskQuotes"

            try {
                Invoke-DotNetCommand -Arguments @("run", "--project", ".\DeskQuotes.csproj")
            }
            finally {
                Pop-Location
            }
        }
        "tests" {
            Invoke-DotNetCommand -Arguments @("test", ".\DeskQuotes.slnx", "--nologo")
        }
        "unit-tests" {
            Invoke-DotNetCommand -Arguments @("test", ".\tests\DeskQuotes.UnitTests\DeskQuotes.UnitTests.csproj", "--nologo")
        }
        "e2e-tests" {
            $e2eProjects = @(Get-ChildItem -Path ".\tests" -Recurse -Filter *.csproj | Where-Object { $_.BaseName -match "(?i)(e2e|playwright)" })

            if ($e2eProjects.Count -eq 0) {
                Write-Host "No E2E test project was found under .\tests. Skipping e2e-tests."
                return
            }

            foreach ($project in $e2eProjects) {
                Invoke-DotNetCommand -Arguments @("test", $project.FullName, "--nologo")
            }
        }
    }
}
finally {
    Pop-Location
}
