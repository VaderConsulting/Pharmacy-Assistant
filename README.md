# Pharmacy Assistant

Manages all Pharmacy website and Product data. Pharmacy Assistant 1.3.5.0 is a C# .NET 3.5 WinForms program that logs on against SQL Server, then edits catalogs, products, stores, documents, events, tasks, and user accounts for the Savemor pharmacy site, publishes files over FTP, and checks for updates via AutoUpdater.NET. Companion RPM Import loads RPM/Corum pricing into the product database; this 2013 working copy from Dave Robinson / VaderConsulting predates Pharmacy Assist.

**Source last updated:** 2013-09-09  
**Language:** C#  
**Target:** .NET 3.5 (CLR 2.0)  
**Output:** WinForms exe

## Solution structure

| Project | Language | Type | Purpose |
|---------|----------|------|---------|
| `Pharmacy Assistant` | C# | WinForms exe (.NET 3.5) | Main Savemor pharmacy website and product manager (logon, catalogs, products, documents, events, tasks, user accounts, FTP, AutoUpdate 1.3.5.0) |
| `Model` | C# | class library (.NET 3.5) | Domain types (Product, Store, Document, Task, Role, Condition) |
| `RPM Import` | C# | WinForms exe (.NET 3.5) | Import RPM data into the Pharmacy Assistant database |
| `Pharmacy Assistant Setup` | InstallShield | setup | Installer for Pharmacy Assistant |
| `RPM Import Setup` | InstallShield | setup | Installer for RPM Import |

## How to open

Open `Pharmacy Assistant.sln` in Visual Studio (VS 2012 solution). The sln also references sibling Historical Dev folders via `..\` that are other repos, not this tree: `Zeta HTML Edit Control`, `i00SpellCheck`, `Linqkit`, `Core`, `System.Windows.Forms.Calendar`, and `RecurranceGenerator`. Connection strings, FTP defaults, and generated settings are gitignored; copy the matching `*.example` files.

## Attribution and provenance

Dave Robinson / VaderConsulting. Assembly title Pharmacy Assistant; assembly company Vader Consulting; assembly copyright 2013 Vader Consulting. Assembly description records calendar code from Jose Menendez Póo (CodeProject, http://www.codeproject.com/Articles/38699/A-Professional-Calendar-Agenda-View-That-You-Will). AutoUpdate feed at http://logonengine.com/AutoUpdate/PharmacyAssistant/ reports version 1.3.5.0. NuGet packages in `packages/` are gitignored. See `THIRD_PARTY_NOTICES.md`.

## License

MIT License. Copyright (c) 2026 VaderConsulting. See `LICENSE`.
