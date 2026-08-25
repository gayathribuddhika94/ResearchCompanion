# Research Companion — Prototype

An ASP.NET Core Razor Pages implementation of the MSc-scope prototype described in the
System Outline and Technical Design companion documents: guided intake (Module 1),
multi-database topic discovery (Module 2), a unified literature library (Module 3),
and basic proposal generation (Module 5).

**Important:** this code was generated in a sandboxed environment with no .NET SDK
available, so it could **not** be compiled or run here to verify it builds. Review it
in Visual Studio as your first step, and treat the notes below as the checklist for
getting it running.

## What's included

- Single ASP.NET Core Razor Pages project (`ResearchCompanion.csproj`) rather than the
  five-project layered solution shown in the Technical Design document — this keeps a
  first working prototype simple to open and run. The `Data/`, `Services/`, and `Pages/`
  folders map to Core / Infrastructure+Application / Web respectively, so splitting them
  into separate class libraries later is a mechanical refactor, not a rewrite.
- ASP.NET Core Identity with three roles (`Researcher`, `Mentor`, `Admin`), seeded on
  startup. Registration currently only builds the Researcher flow.
- EF Core + SQL Server models matching the Technical Design schema (Section 5), minus
  Institutions/Templates/Milestones/MentorAssignments/DocumentVersions, which are
  documented but not yet implemented — add them the same way as the existing entities
  when you're ready to extend the prototype.
- Three real API integrations (Semantic Scholar, CrossRef, arXiv) behind one
  `IExternalSearchProvider` interface, with de-duplication in `LiteratureAggregationService`.
- A rule-based topic-scoring heuristic in `TopicDiscoveryService` (explicitly a
  placeholder for later LLM-based novelty scoring — see the Technical Design document).
- A working `.docx` proposal generator (`DocumentRenderer`) that fills
  `Templates/Proposal_Template.docx` by replacing `{{TokenName}}` placeholders — open
  that file in Word to see/change the template structure.

## Getting it running (Visual Studio 2022)

1. Open `ResearchCompanion.sln`.
2. Let NuGet restore the packages listed in `ResearchCompanion.csproj`. If any version
   fails to resolve, use Manage NuGet Packages and accept the latest compatible 8.x
   version — exact patch numbers drift over time.
3. Confirm the connection string in `appsettings.json` — the default targets SQL Server
   LocalDB, which ships with Visual Studio, so no setup should be required for local dev.
4. Open the Package Manager Console and run:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
5. Press F5. Register an account, then walk through: Intake → Topics/Suggestions →
   Library (search and save a few papers) → Documents/GenerateProposal.
6. Open the downloaded `Proposal.docx` and confirm each `{{Token}}` placeholder was
   replaced — an unreplaced token usually means a typo between the token name in
   `GenerateProposalModel.cs` and the template.

## Known gaps / where to go next

- No automated tests yet (`ResearchCompanion.Tests` from the Technical Design document
  isn't included in this drop — add an xUnit project and start with
  `LiteratureAggregationService.Deduplicate`, since it's pure logic with no I/O).
- `TopicDiscoveryService`'s scoring is a simple heuristic, not the clustering/semantic
  approach described as a future enhancement in the System Outline.
- IEEE Xplore and Scopus are not integrated (licensed API access required — see
  Technical Design, Section 7). Add a new class implementing `IExternalSearchProvider`
  and register it in `Program.cs` once you have access.
- Mentor/supervisor review screens, presentation generation, and the progress dashboard
  are designed in the System Outline (Modules 6–9) but not built here — out of scope for
  the initial MSc prototype per Section 12.

See the **Prototype Build Tutorial** document for the original step-by-step reasoning
behind each of these pieces, and the **Technical Design** document for the full,
un-simplified five-project architecture this can grow into.
