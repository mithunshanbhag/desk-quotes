param(
    [ValidateSet("app", "tests", "unit-tests")]
    [string]$target = "app"
)

switch ($target) {
    "app" {
        dotnet run --project ./src/DeskQuotes/DeskQuotes.csproj
    }
    "tests" {
        dotnet test ./DeskQuotes.slnx
    }
    "unit-tests" {
        dotnet test ./tests/DeskQuotes.UnitTests/DeskQuotes.UnitTests.csproj
    }
    default {
        Write-Host "Invalid target specified. Please use one of: app, tests, unit-tests."
    }
}