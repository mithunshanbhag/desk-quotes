# Learnings

- Runtime services are defined under `src\DeskQuotes\Services\Implementations`; tests should import that namespace rather than the removed legacy `DeskQuotes.Services` duplicates to avoid ambiguous type references.
