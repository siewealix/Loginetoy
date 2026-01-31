# PcCleaner

PcCleaner est une application Windows **safe cleaner** pour Windows 10/11, conçue pour le nettoyage local et transparent d’un PC. L’objectif est d’offrir un outil moderne, fiable et maintenable, avec un **mode Simulation (dry-run) activé par défaut** et des garde-fous stricts (pas de registry cleaner).

## ✨ Fonctionnalités
- Analyse (scan) asynchrone avec estimation de l’espace récupérable.
- Nettoyage optionnel après confirmation explicite.
- Catégories configurables :
  - Fichiers temporaires utilisateur (`%TEMP%`, `AppData\Local\Temp`).
  - Fichiers temporaires Windows (`C:\Windows\Temp`) — **admin requis**.
  - Cache des miniatures (`thumbcache*.db`).
  - Corbeille (estimation + vidage).
  - Logs applicatifs (`*.log`) dans des dossiers configurables.
  - Dossiers personnalisés (whitelist).
- Exclusions utilisateur (patterns ou chemins).
- Journalisation locale (Serilog) dans `%AppData%\PcCleaner\Logs`.
- Rapport post-exécution : résumé, erreurs, audit local.

## ✅ Sécurité & Éthique
- **Aucune collecte de données**, aucune exfiltration.
- **Dry-run par défaut** : estimation sans suppression.
- **Confirmation explicite** avant toute suppression.
- **Pas de registry cleaner** (trop risqué).
- **Allow-list** stricte + exclusion des chemins système critiques.
- Symlinks/junctions ignorés pour éviter toute traversée non sûre.

## 🧱 Architecture
- **.NET 8**
- **WPF + MVVM**
- **DI** (Microsoft.Extensions.DependencyInjection)
- **Serilog** (fichiers log tournants)
- **xUnit** pour le moteur et les règles de sécurité

Arborescence :
```
PcCleaner.sln
src/
  PcCleaner.App/            # UI WPF
  PcCleaner.Core/           # Moteur (scan/clean, règles, sécurité)
  PcCleaner.Infrastructure/ # Accès système / IO / Windows
tests/
  PcCleaner.Core.Tests/
```

## 🚀 Build & Run
### Prérequis
- Windows 10/11
- .NET SDK 8.x
- Visual Studio 2022 (optionnel)

### Build
```bash
# À la racine du repo
# (Solution : PcCleaner.sln)
```

### Run
- Ouvrir `PcCleaner.sln` dans Visual Studio.
- Démarrer `PcCleaner.App`.

## 🧪 Tests
```bash
dotnet test tests/PcCleaner.Core.Tests
```

## 🧩 Configuration
Le fichier `src/PcCleaner.App/appsettings.json` permet de personnaliser :
- `CustomFolders` : dossiers personnalisés (whitelist).
- `LogFolders` : dossiers pour nettoyer les `.log`.
- `Exclusions` : patterns (`*.log`) ou chemins entiers.

## 📦 Packaging (MSIX recommandé)
- Utiliser Visual Studio + Windows Application Packaging Project.
- Générer un MSIX signé pour la distribution.
- Alternative : MSIX via `dotnet publish` + `makeappx` (documenté dans la doc MSIX).

## ♻️ Legacy
Aucun dossier `legacy/` n’a été trouvé. Les anciennes sources présentes à la racine du dépôt ne sont pas modifiées dans cette solution modernisée.

## ⚠️ Limitations connues
- Certaines catégories nécessitent les droits admin (ex : `C:\Windows\Temp`).
- Le nettoyage de la corbeille dépend des APIs Windows (P/Invoke).

## 🗺️ Mapping des fonctionnalités legacy
- **Registry cleaner** : supprimé (trop risqué), remplacé par nettoyage de fichiers temporaires.
- **Nettoyage de fichiers temporaires** : conservé et modernisé (dry-run + garde-fous).
