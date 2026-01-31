# Changelog

## [Unreleased]
### Added
- Nouvelle solution .NET 8 avec architecture WPF/MVVM et séparation Core/Infrastructure.
- Moteur de scan/nettoyage avec dry-run par défaut.
- Catégories sécurisées (temporaires, miniatures, corbeille, logs, dossiers custom).
- Logs Serilog locaux + rapport d’exécution.
- Tests xUnit sur les règles de sécurité et les catégories.

### Changed
- Suppression explicite des fonctionnalités de registry cleaner (remplacé par nettoyage de fichiers temporaires).
