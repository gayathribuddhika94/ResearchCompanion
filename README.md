# Research Companion — Prototype

An ASP.NET Core Razor Pages implementation of the MSc-scope prototype described in the
System Outline and Technical Design companion documents: guided intake (Module 1),
multi-database topic discovery (Module 2), a unified literature library (Module 3),
and basic proposal generation (Module 5) — extended with literature-review curation,
IEEE citation formatting, an in-system documents library, and Mendeley import.

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
- **Mendeley import** — connect a Mendeley account (OAuth 2.0) and pull existing library
  documents into the local literature library. See "Connecting Mendeley" below.
- **Literature-review curation** — each saved paper has an "Include in literature review"
  toggle and a free-text personal-notes field (`Pages/Library/Index.cshtml`), so a
  researcher can keep reference material without every saved paper being cited.
- **IEEE citation formatting** — `CitationFormatterService` renders saved, review-flagged
  papers as numbered IEEE-style references for the generated document's References
  section. APA7 can be added the same way as a second `FormatApa` method later.
- **In-system documents library** — generating a proposal no longer forces an immediate
  browser download. `Documents/GenerateProposal` now saves the rendered `.docx` to disk
  and records it in the database; `Pages/Documents/Index.cshtml` lists every generated
  document, lets you read each section inline, and downloads the file only when you
  explicitly ask for it.
- A rule-based topic-scoring heuristic in `TopicDiscoveryService` (explicitly a
  placeholder for later LLM-based novelty scoring — see the Technical Design document).
- A working `.docx` proposal generator (`DocumentRenderer`) that fills
  `Templates/Proposal_Template.docx` by replacing `{{TokenName}}` placeholders, including
  multi-line values (e.g. a numbered reference list) rendered as real line breaks rather
  than run-on text.

## Getting it running (Visual Studio 2022)

1. Open `ResearchCompanion.sln`.
2. Let NuGet restore the packages listed in `ResearchCompanion.csproj`. If any version
   fails to resolve, use Manage NuGet Packages and accept the latest compatible 8.x
   version — exact patch numbers drift over time. No new NuGet packages were added for
   the features above — Sessions and Options binding both ship in the ASP.NET Core
   shared framework already referenced by the Web SDK.
3. Confirm the connection string in `appsettings.json` — the default targets SQL Server
   LocalDB, which ships with Visual Studio, so no setup should be required for local dev.
4. Open the Package Manager Console and run:
   ```
   Add-Migration AddLiteratureReviewNotesDocumentPathAndMendeley
   Update-Database
   ```
   This migration is **not included** in this drop since EF's tooling isn't available in
   the sandbox this was written in — running `Add-Migration` yourself generates it
   correctly from the updated entity classes (`UserLibraryItem.IncludeInLiteratureReview`,
   `ResearchDocument.Title`/`FilePath`, and the new `MendeleyAccount` table).
5. Press F5. Register an account, then walk through: Intake → Topics/Suggestions →
   Library (search, save a few papers, tick "Include in literature review", add notes)
   → Documents/GenerateProposal.
6. Open the "My documents" page from the nav bar — your generated proposal is listed
   there with its sections readable inline, and a Download button when you want the file.

## Connecting Mendeley

1. Sign in at [dev.mendeley.com/myapps.html](https://dev.mendeley.com/myapps.html) with
   your Mendeley account and register a new app.
   - **Redirect URL**: must exactly match `Mendeley:RedirectUri` in `appsettings.json`
     (defaults to `https://localhost:7100/Mendeley/Callback` — check
     `Properties/launchSettings.json` if your dev port differs).
2. Copy the generated **Client ID** and **Client Secret**. Put the Client ID in
   `appsettings.json` under `Mendeley:ClientId`; put the **Client Secret in user secrets**
   instead of committing it to source control:
   ```
   dotnet user-secrets set "Mendeley:ClientSecret" "your-secret-here"
   ```
   (Run this from the project folder, or use Visual Studio's "Manage User Secrets".)
3. Run the app, go to **Mendeley** in the nav bar, and click **Connect Mendeley account**.
   You'll be redirected to Mendeley's own sign-in/consent screen, then back to
   `/Mendeley/Import`, which lists your Mendeley library with a **Save to library** button
   per item.
4. Access tokens are short-lived; `Pages/Mendeley/Import.cshtml.cs` refreshes them
   automatically using the stored refresh token, so you shouldn't need to reconnect often.

This was written against the OAuth 2.0 Authorization Code flow documented at
dev.mendeley.com as of this writing — if Mendeley has changed its API since, the error
message returned by `MendeleyClient` (surfaced on the Import page) is the first place to
check.

## Known gaps / where to go next

- No automated tests yet (`ResearchCompanion.Tests` from the Technical Design document
  isn't included in this drop — add an xUnit project and start with
  `LiteratureAggregationService.Deduplicate`, since it's pure logic with no I/O).
- `TopicDiscoveryService`'s scoring is a simple heuristic, not the clustering/semantic
  approach described as a future enhancement in the System Outline.
- IEEE Xplore and Scopus are not integrated (licensed API access required — see
  Technical Design, Section 7). Add a new class implementing `IExternalSearchProvider`
  and register it in `Program.cs` once you have access.
- Only IEEE citation formatting is implemented (`CitationFormatterService.FormatIeee`).
  Add a matching `FormatApa` method the same way if you need APA7 as well.
- Mentor/supervisor review screens, presentation generation, and the progress dashboard
  are designed in the System Outline (Modules 6–9) but not built here — out of scope for
  the initial MSc prototype per Section 12.
- The Mendeley integration hasn't been exercised against a live Mendeley app (no network
  access in the environment this was written in) — the OAuth flow, token refresh, and
  document-mapping logic follow the published API docs closely, but budget time to debug
  the first real connection attempt.

See the **Prototype Build Tutorial** document for the original step-by-step reasoning
behind each of these pieces, and the **Technical Design** document for the full,
un-simplified five-project architecture this can grow into.
