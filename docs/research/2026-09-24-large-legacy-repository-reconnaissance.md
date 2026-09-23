# Sanitized Real-Project Repository Reconnaissance

Date recorded in PKC research: 2026-09-24
Status: SANITIZED / SHAREABLE DESIGN EVIDENCE

This document records a user-provided sanitized reconnaissance of a real large mixed legacy enterprise repository. It is retained as research evidence for Repository Discovery and Scan Planning. It contains aliases and approximate counts only; the private source repository and unsanitized reconnaissance are not part of PKC.

Important use boundary:

- treat this as a real-project design/acceptance oracle, not executable source truth;
- do not hard-code aliases, counts, folder names or application shapes from this report;
- private-source validation must remain in the approved environment;
- benchmark reports derived from it must remain sanitized;
- uncertainty/UNKNOWN findings are intentional evidence and must not be normalized away;

---

# Sanitized Repository Profile

Purpose: a shareable, sanitized summary of a bounded reconnaissance of a large mixed legacy enterprise repository, for PKC scan-design discussion.
All application, module and system identities are replaced with stable aliases. Counts are approximate.

- **Scale:** ~25k tracked files. Approximate mix: ~13k C#, ~2.6k JavaScript, ~2.2k TypeScript, ~1k Razor views, ~1k HTML templates, ~700 SCSS, ~700 resource files.
  On disk the repository is larger because of untracked build output and package caches.
- **Projects:** ~65–70 .NET projects in a single solution file (one project is orphaned from the solution), plus 2 frontend workspaces and 2 extension packages for an external ERP written in a non-.NET vendor DSL.
- **Architecture style:** a modular-monolith .NET backend, with an older product stack living in the same repository that is being migrated into it gradually, module by module.
  Two backend module generations coexist:
  - **Compile-time feature modules**, referenced by project and registered explicitly in the host.
  - **Runtime plugin modules**, loaded by reflection from an output subfolder and copied there by a custom post-build step. They are not project-referenced by the host.
- **Runtime applications:** 18 likely deployables (APIs, a legacy MVC web app, SPAs, workers, integrations, a gateway, a local-dev orchestrator, ERP extensions) plus 1 tooling console app.
- **Age / legacy:**
  - The legacy product's history begins roughly a decade ago. Its web assets were relocated during a framework upgrade about a year before analysis, so pre-move history is not visible at the current paths.
  - The legacy UI still ships very old browser shims and a jQuery-era commercial admin theme.
  - Two UI generations coexist for the same product: legacy MVC plus jQuery, and a newer Angular SPA that links back to the MVC app.

# Detected Application Types

| Alias | Role | Technology stack | Scan relevance | Confidence |
|---|---|---|---|---|
| WEB_API_A | web-api / composition root of the modular monolith | ASP.NET Core Web API, Autofac, MediatR plus a custom mediator/event bus, EF Core, Dapper, FluentValidation, Serilog | Primary. Defines which modules exist at runtime | HIGH |
| MVC_UI_A (includes LEGACY_JS_UI_A) | aspnet-mvc-web, legacy back-office UI | ASP.NET Core MVC and Razor (runtime compilation), server-side bundler, jQuery, Bootstrap 3, COMMERCIAL_THEME_A, CKEditor 4 and 5, many jQuery plugins | Very large. Mostly vendor assets by file count | HIGH |
| WEB_API_B | web-api (legacy product API, probably backing ANGULAR_UI_B) | ASP.NET Core Web API, Newtonsoft JSON, OIDC auth | Later wave | HIGH (app); MEDIUM (UI link) |
| WEB_API_C | web-api (backend for the customer-facing portal) | ASP.NET Core Web API | Later wave | HIGH |
| WEB_API_D | web-api (mobile client backend) | ASP.NET Core Web API | Later wave | HIGH |
| INTEGRATION_A | integration web-api for external consumers | ASP.NET Core Web API | Later wave | HIGH |
| INTEGRATION_B | integration service to EXTERNAL_SYSTEM_B (external reporting service) | ASP.NET Core, depends on VENDORED_CLIENT_A | Later wave | HIGH |
| WORKER_A | background job host for the legacy stack | ASP.NET Core with Hangfire | Later wave | HIGH |
| WORKER_B | scheduled ETL worker from EXTERNAL_SYSTEM_A (ERP) into DATABASE_B (reporting store) | .NET Worker, Windows Service, Quartz, EF Core (two contexts) | Later wave | HIGH |
| WORKER_C | scheduled jobs (document exchange, email) | ASP.NET Core, Quartz with dashboard, Razor Pages login | Small. Deployment target UNKNOWN | MEDIUM |
| GATEWAY_A | reverse proxy and OpenAPI merge in front of WEB_API_A and an integration API | YARP | Small. Production role UNKNOWN | MEDIUM |
| HOST_A | local-dev orchestrator only (not production composition) | .NET Aspire AppHost, with a cache, a message broker and a mail catcher as containers | LIGHT_INDEX (topology evidence) | HIGH |
| ANGULAR_UI_A | admin SPA for SHARED_MODULE_P1's domain | Angular (current major), COMMERCIAL_THEME_B, ng-bootstrap, PrimeNG, ngx-datatable | Second wave | HIGH |
| ANGULAR_UI_B | new SPA replacing MVC_UI_A screens (most active UI) | Angular (current major), PrimeNG, Tailwind, MSAL, ngx-translate, UI_TEMPLATE_A | First wave | HIGH |
| ANGULAR_UI_C | customer-facing portal SPA | Angular (current major), PrimeNG, Tailwind, UI_TEMPLATE_A | Later wave | HIGH |
| ANGULAR_UI_D | warehouse/handheld SPA, in a **separate workspace** | Angular (~3 majors older), npm, rxjs-compat, Karma/Protractor, built on an open-source starter scaffold | Later wave | HIGH |
| ERP_EXTENSION_A | extension package deployed inside EXTERNAL_SYSTEM_A (custom API pages, tables, event subscribers) | ERP vendor DSL (non-.NET) | Needs a DSL-aware scanner | MEDIUM |
| ERP_EXTENSION_B | regional variant of ERP_EXTENSION_A (partly diverged: ~6 shared files differ, ~11 unique files per side) | ERP vendor DSL | Same as above | MEDIUM |
| TOOLING_A | one-off data migration console (legacy → SHARED_MODULE_P1) | .NET console, Dapper, SQL scripts | LIGHT_INDEX, no production authority | HIGH |

Libraries whose names look like applications, but are not:
- **SHARED_LIB_A** is named like an API but is a plain library with a framework reference. It is referenced by WEB_API_A and SHARED_MODULE_P2. (HIGH)
- **SHARED_MODULE_L1** is a legacy business module whose web part is a Razor class library consumed by MVC_UI_A. (HIGH)
- **SHARED_CORE_B_SETUP** is a shared host-setup library (Web SDK, library output) used by the legacy hosts. (HIGH)

# Technology Stack

- **Backend:** modern .NET (single current target framework; one orphaned project targets an older one), ASP.NET Core Web API and MVC, Razor Pages, Razor class libraries, EF Core (SQL Server), Dapper, Autofac, MediatR, WolverineFx, FluentValidation, AutoMapper, Swagger/OpenAPI, API versioning, Serilog, Application Insights, OpenTelemetry, Polly, Quartz, Hangfire, RabbitMQ, YARP, .NET Aspire, a Roslyn source generator, Scriban templating, PDF/Excel generation libraries, WCF client proxies.
- **Frontend (new):** Angular (current major, esbuild-based application builder), pnpm, TypeScript, PrimeNG, Tailwind, ng-bootstrap, Angular CDK, ngx-datatable, MSAL, ngx-translate, CKEditor 5 (npm), FullCalendar, Chart.js, Quill, Vitest, ESLint, Prettier.
- **Frontend (older Angular):** an Angular workspace ~3 majors older, npm lockfile, rxjs-compat, Karma, Protractor.
- **Frontend (legacy):** Razor views with inline scripts, jQuery 2.x, jQuery UI, jQuery Validation and unobtrusive AJAX, Bootstrap 3, CKEditor 4 (old 4.x) and CKEditor 5 (UMD build), select2, dropzone, echarts, Chart.js, flot, moment, jstree, typeahead, linq.js, and many small jQuery plugins, plus several commercial libraries (aliased below).
- **Other languages:** a proprietary ERP extension DSL.
- **Build/CI:** MSBuild with central package management, Azure DevOps YAML pipelines (one CD pipeline per deployable, Docker images), Cake build scripts, GitVersion, a server-side bundling/minification config for the legacy UI.

# Sanitized Application / Dependency Map

Arrows point from consumer to dependency. Built only from project references, host registration and config key names.

```
HOST_A (local dev only)
  -> WEB_API_A, WEB_API_B, WEB_API_C, INTEGRATION_A, WORKER_A, MVC_UI_A, GATEWAY_A
  -> cache container, message broker container, mail-catcher container
  (project-references WEB_API_D but never registers it)

WEB_API_A
  -> SHARED_MODULE_SET_A (compile-time feature modules, ~10 business modules + 1 supporting module + contracts)
       each feature module -> SHARED_MODULE_SUPPORT_A -> SHARED_CORE_A
  -> DATABASE_MIGRATIONS_A (schema history for part of SHARED_MODULE_SET_A)
  -> SHARED_LIB_A -> one feature module
  -> [runtime reflection load, no project reference]
       SHARED_MODULE_P1 (DDD/CQRS plugin: domain, command, query, persistence, migrations)
             -> depends on a feature module and its contracts
       SHARED_MODULE_P2 (integrations plugin, scheduled jobs)
             -> SHARED_MODULE_P1, SHARED_LIB_A, EXTERNAL_SYSTEM_C (accounting SaaS SDK)

GATEWAY_A -> (HTTP) WEB_API_A, integration API

MVC_UI_A -> SHARED_CORE_B (large legacy services/domain/infrastructure layers) -> SHARED_CORE_A
         -> SHARED_MODULE_L1 (Razor class library)
         -> one feature module from SHARED_MODULE_SET_A (compiled in)
         -> SHARED_CORE_B_SETUP -> SHARED_MODULE_SUPPORT_A
         -> LEGACY_JS_UI_A (custom page scripts) -> jQuery, COMMERCIAL_GRID_A, COMMERCIAL_THEME_A,
            CKEditor 4 (modified) / CKEditor 5, COMMERCIAL_VIEWER_A, COMMERCIAL_UI_LIB_A, other plugins
SHARED_CORE_B -> VENDORED_CLIENT_A -> EXTERNAL_SYSTEM_B
WEB_API_B / WEB_API_C -> SHARED_CORE_B + SHARED_CORE_A + feature modules from SHARED_MODULE_SET_A
WORKER_A -> SHARED_CORE_B, SHARED_MODULE_L1
INTEGRATION_B -> SHARED_CORE_A, SHARED_CORE_B_SETUP, EXTERNAL_SYSTEM_B

ANGULAR_UI_B -> (HTTP) WEB_API_B [MEDIUM] + WEB_API_A [MEDIUM]; hyperlinks back into MVC_UI_A
ANGULAR_UI_A -> (HTTP) WEB_API_A + WEB_API_B (+ further base URLs, one for a module absent from the repo)
ANGULAR_UI_C -> (HTTP) WEB_API_C [MEDIUM, inferred from a same-origin path]
ANGULAR_UI_D -> (HTTP) UNKNOWN (runtime-injected config); route names suggest SHARED_MODULE_P1's domain [LOW]
ANGULAR_UI_A/B/C -> SHARED_UI_LIB_A (workspace library, heavily imported by all three)

WORKER_B -> EXTERNAL_SYSTEM_A (source) -> DATABASE_B (target)
ERP_EXTENSION_A/B -> run inside EXTERNAL_SYSTEM_A; which .NET code consumes their custom API pages is UNKNOWN
```

Key structural observations:
- Some feature modules are compiled into **both** product stacks, so their ownership is shared.
- The plugin modules are reachable from WEB_API_A only through reflection and a build copy step. A reference-based graph would miss them.

# First-Party Runtime Areas

- **WEB_API_A:** host setup, middleware, module registration, routing conventions, small template assets. Small (~40 files). HIGH.
- **SHARED_MODULE_SET_A:** ~12 projects, ~900 C# files. The newest backend generation with the highest recent commit activity. HIGH.
- **SHARED_MODULE_P1 / SHARED_MODULE_P2:** ~1.3k files together. Layered plugin modules; one module's application layer sits outside its module folder, at the repository root. HIGH.
- **WORKER_B, WORKER_C, GATEWAY_A, SHARED_LIB_A:** small to medium. HIGH.
- **MVC_UI_A server side:** ~100 controller files, ~900 Razor views in ~90 view folders, ~650 model files, resources. HIGH.
- **LEGACY_JS_UI_A:** ~320 first-party page scripts plus ~10 first-party helper and patch scripts (see the JavaScript section). HIGH.
- **Legacy API/job/integration hosts:** WEB_API_B/C/D, INTEGRATION_A/B, WORKER_A. HIGH.
- **SHARED_MODULE_L1:** ~460 files, including ~60 views and ~30 scripts. HIGH.
- **ANGULAR_UI_A/B/C/D application code:** roughly 600 / 850 / 170 / 170 non-spec TS files. HIGH, except ANGULAR_UI_D's split between scaffold and business code, which is MEDIUM.
- **ERP_EXTENSION_A/B:** ~30 DSL files each. HIGH.
- **FIRST_PARTY_SHARED:**
  - SHARED_CORE_A (~1.1k files, 5 projects including a source generator and a migrations project).
  - SHARED_CORE_B (~5k files; its largest layer is ~2.7k files with the highest commit activity in the repo).
  - SHARED_UI_LIB_A (~170 files).
  - A workspace-level shared style folder.

# Third-Party Runtime Assets

Commercial or purchased products are aliased. Unless noted, all of these belong to MVC_UI_A / LEGACY_JS_UI_A.
Note: history at these asset paths only starts at a relocation about a year before analysis, so a single commit does not prove an asset is unmodified.

| # | Asset | Kind | Runtime relevance | Locally modified | Confidence |
|---|---|---|---|---|---|
| T1 | CKEditor 4 (old 4.x) | rich-text editor, ~350 files | YES, ~30 views | **YES (THIRD_PARTY_MODIFIED):** a custom plugin added inside the vendor tree, edits to the core bundle, and added image assets | HIGH |
| T2 | CKEditor 5 (UMD build) | rich-text editor | YES, a few views | UNKNOWN | MEDIUM |
| T3 | COMMERCIAL_THEME_A (jQuery/Bootstrap 3 generation) | admin theme: layout scripts, CSS, images | YES, the main layout | PARTIAL. Company branding images are mixed in, some theme scripts have domain-specific names, and bundle outputs live inside the theme folder | MEDIUM |
| T4 | Theme plugin pack (~2.6k files, ~24 plugins) | charts, form widgets, select, date pickers, dropzone, alerts, maps, etc. | YES for most, via the theme's global bundle or direct layout tags. One rich-text editor plugin has no reference found | Only T1 | HIGH |
| T5 | COMMERCIAL_CHART_A, 2 copies (~1.4k files each; the copies differ) | charting | One copy is bundled. The other has no reference found | UNKNOWN | MEDIUM |
| T6 | COMMERCIAL_VIEWER_A | in-browser PDF viewer, ~380 files | YES, the layout and ~12 views | NO evidence | HIGH |
| T7 | jQuery 2.x, jQuery UI, jQuery Validation, unobtrusive AJAX | core libraries | YES, the layout and bundles | NO evidence | HIGH |
| T8 | COMMERCIAL_GRID_A | jQuery data grid plus locale files | YES, ~160 views and ~70% of page scripts | UNKNOWN (a local wrapper exists alongside it) | HIGH |
| T9 | Bootstrap 3 and plugins (select, tags input, locale packs) | UI framework | YES, bundled | NO evidence | HIGH |
| T10 | Generic jQuery plugin folder (~160 files: date pickers, charts, input masks, zip/file-saver, file manager, etc.) | assorted | YES, partly bundled or in the layout | UNKNOWN (the file-manager plugin may be customised) | MEDIUM |
| T11 | COMMERCIAL_UI_LIB_A | component suite (minified bundle and globalization data) | YES, a few views | NO evidence | HIGH |
| T12 | COMMERCIAL_GANTT_A | gantt chart | YES, 1 view | UNKNOWN | HIGH |
| T13 | XSLT runtime plus e-document viewer (VENDOR_VIEWER_B) | e-document rendering | YES, 1 view | The viewer script's ownership is UNKNOWN | MEDIUM |
| T14 | Assorted small jQuery-era libraries (linq.js, moment, typeahead, jstree, flatpickr, tooltips, templating, cookies, time pickers, dialogs, one unidentified library) | assorted | YES, several bundled | NO evidence (one library is unidentified) | MEDIUM |
| T15 | Legacy Content folder (~720 files: vendor CSS, fonts, images, site CSS) | styles/assets | YES | Mixed first-party and vendor content (UNKNOWN split) | LOW |
| T16 | COMMERCIAL_THEME_B (Angular edition, same vendor family as T3) inside ANGULAR_UI_A's app source, plus ~370 media files | admin theme compiled with the app | YES, the app shell | UNKNOWN, probably customised | MEDIUM |
| T17 | UI_TEMPLATE_A (PrimeNG-based application template shell) in ANGULAR_UI_B/C | layout shell | YES | UNKNOWN, probably customised | LOW |
| T18 | VENDORED_CLIENT_A | vendored API client with generated WCF proxies and hand-written wrappers | YES, via SHARED_CORE_B | UNKNOWN (THIRD_PARTY_UNKNOWN) | MEDIUM |
| T19 | Open-source Angular starter scaffold under ANGULAR_UI_D | build scaffold, repo-meta files | Only as a scaffold | YES, the app is built on it | MEDIUM |
| T20 | npm / NuGet restored packages | dependencies | YES | n/a (restorable) | HIGH |

# Custom / Legacy JavaScript

- **Amount:** ~2.6k JS files in the legacy UI. Only ~350 of them are first-party: ~320 page scripts, ~10 helpers and patches, and ~30 in the Razor class library. The rest is vendor. There are also ~330 `.min.js` files repo-wide.
- **Mixing:** first-party and vendor files sit side by side in the same top-level scripts folder. Path-level classification is insufficient, and a file-level allowlist is needed.
- **Page scripts:** organised into ~20+ business-area subfolders. Rough marker counts across ~320 files:
  - AJAX calls to MVC controllers in ~50%.
  - Grid configuration in ~70%.
  - DOM event handlers in ~65%.
  - Select-widget initialisation in ~8%.
  - Rich-text editor usage in ~5%.
  - Client-side localisation text files for 3 cultures.
- **Wrapper/patch patterns:**
  - a helper that overrides a validation rule for locale-specific decimals;
  - a probable grid wrapper;
  - common AJAX/dialog helpers;
  - global search and notification scripts;
  - a service-worker client.
- **Inline scripts:** present in the main layout. The extent across ~900 views is UNKNOWN.
- **Generated/bundled:** 8 bundles are defined in a bundler config, and their outputs are tracked. The biggest global bundle (~70 inputs) **mixes first-party scripts with vendor libraries**, and the layout loads the bundle rather than the sources.
- **Modified vendor:** the CKEditor 4 tree contains a first-party plugin, so the customisation boundary lies inside a vendor directory.
- **Classification difficulty:**
  - Vendor libraries can look unreferenced from views while actually being loaded via bundles.
  - Theme script folders contain some domain-named scripts.
  - A few scripts have unclear ownership.
  - Duplicate library copies exist.

# Source Classification

| Scan mode | Areas |
|---|---|
| **DEEP_SCAN** | WEB_API_A; SHARED_MODULE_SET_A; SHARED_MODULE_P1/P2 (second wave); WORKER_B, WORKER_C, GATEWAY_A (later); MVC_UI_A server side (later); LEGACY_JS_UI_A first-party page scripts, helpers and patches; the custom plugin files inside the modified CKEditor 4 tree; WEB_API_B/C/D, INTEGRATION_A/B, WORKER_A, SHARED_MODULE_L1 (later, one per wave); ANGULAR_UI_A/B/C/D app code (excluding template shells); ERP_EXTENSION_A/B (DSL-aware scanner needed) |
| **TARGETED_SCAN** | SHARED_CORE_A; SHARED_CORE_B (follow only from entry points); SHARED_LIB_A; SHARED_UI_LIB_A and workspace shared styles; the hand-written wrappers inside VENDORED_CLIENT_A (if its flows are in scope); theme scripts with domain-specific names |
| **RUNTIME_DEPENDENCY_INDEX** | T1 (rest of tree), T2–T19: record the library, its generation, and which layouts, views or bundles load it. Do not analyse internals |
| **LIGHT_INDEX** | HOST_A; the solution file; MSBuild props and central package files; pipelines, dockerfiles, build scripts; bundler config and its tracked outputs; EF migration projects and folders (authoritative schema history, but generated); generated XML API docs; designer files; TOOLING_A |
| **TEST_EVIDENCE** | ~7 test projects (unit, integration, API; containerised SQL; in-memory host); DB bootstrap scripts; an API collection file. No production authority |
| **SAFE_AUTO_EXCLUDE** | untracked build output and caches (see below) |
| **UNKNOWN** | see Uncertain / Legacy Areas |

# Recommended PKC Scan Plan

1. **Topology pass (LIGHT_INDEX first):**
   - Parse the solution file and project references.
   - Parse host registration code, which reveals the explicit feature-module list and the reflection-based plugin loader.
   - Read the local-dev orchestrator, the frontend workspace configs, the pipelines (their path triggers map deployables to folders), and the bundler config (input-to-output lists).
2. **Model plugin edges explicitly.** Add synthetic edges from WEB_API_A to SHARED_MODULE_P1/P2, sourced from the solution's build-dependency declarations and the post-build copy targets.
3. **Treat shared feature modules as multi-owner nodes.** SHARED_MODULE_SET_A members that are compiled into both stacks should carry every owning host.
4. **Backend scanners:** web-api/controller, DI registration, EF model, CQRS handler, scheduled job (Quartz/Hangfire) and integration-event scanners, applied to DEEP_SCAN roots. Shared cores get TARGETED_SCAN, driven by references.
5. **Legacy UI scanners:**
   - Razor view scanner for the server side.
   - A legacy-JS scanner over a **file-level allowlist** of first-party scripts, with an AJAX-call-to-controller linker and grid-config extraction.
   - Bundle-aware mapping, so runtime loads can be traced back to source inputs.
6. **Angular scanners:** routing, HTTP service, component and i18n scanners per app. Handle ANGULAR_UI_D as a separate workspace with its own dependency resolution.
7. **Vendor handling:** RUNTIME_DEPENDENCY_INDEX only. The one known modified vendor tree gets a narrow DEEP_SCAN carve-out for its custom plugin.
8. **Non-.NET:** the ERP extensions need a DSL-aware scanner. Otherwise, index their object names and IDs only.
9. **Tests:** keep them as separate evidence, attached to production nodes by reference, never merged into them.

# Safe Auto Exclusions

Strong evidence only. None of these are tracked in version control; all are ignored or restorable:
- `bin/` and `obj/` build output.
- `node_modules/` package caches (restorable from lockfiles).
- Angular build output and build cache folders (`dist/`, `.angular/`).
- Test results, test agent output, coverage reports, build-script artifact folders.
- IDE state folders, user files, language-server caches, local AI-tool folders and analysis output.

# Areas That Must NOT Be Auto-Excluded

- **The legacy `Scripts` folder:** first-party page scripts and helpers are interleaved with vendor libraries.
- **Vendor-like theme and plugin folders:** one contains a locally modified editor with a custom plugin.
- **Theme and `Content` folders:** they mix theme assets with first-party branding and possibly site CSS.
- **Commercial runtime libraries** (viewer, component suite, gantt): index them, don't exclude them.
- **Tracked bundle outputs:** they are what the layout actually loads, and they contain first-party code.
- **Theme source compiled inside an Angular app, and template layout shells:** these also carry app navigation.
- **The vendored API client:** it is on the production path.
- **EF migration folders and projects:** generated, but they are the authoritative schema history, and they **are** compiled (see PKC Risks).
- **The tooling folder:** it encodes legacy-to-new data-mapping knowledge.
- **ERP extension folders:** non-.NET, but first-party production code.
- **Application-layer projects in unexpected locations** (for example at the repository root instead of under their module).
- **Stray files outside any project:** keep them until their status is known.

# Uncertain / Legacy Areas

Open UNKNOWN / ambiguous categories: **15**.

1. **WORKER_C deployment target:** it has no CD pipeline and is not in HOST_A, but the build script publishes it as an artifact.
2. **GATEWAY_A production role:** it is in HOST_A and published by the build script, but has no CD pipeline.
3. **Orphaned project:** not in the solution, targets an older framework, and references a project that does not exist.
4. **Orphaned test files:** test sources with no project file, so they are never compiled.
5. **Stale frontend workspace entry:** it points to a folder that does not exist; the real app lives in a separate nested workspace.
6. **Duplicate chart library copy:** one copy is used via a bundle; the other has no reference found, but dynamic loading was not ruled out.
7. **Unreferenced rich-text editor plugin** in the theme plugin pack: probably dead.
8. **Stray source files outside any project:** one duplicates the path of a live module file; one has no counterpart anywhere.
9. **UI_TEMPLATE_A:** template identity and degree of customisation are UNKNOWN.
10. **COMMERCIAL_THEME_B in ANGULAR_UI_A:** version and customisation extent are UNKNOWN.
11. **Stale CD path triggers:** they reference module folders that do not exist. An SPA also configures a base URL for one of those absent modules.
12. **VENDORED_CLIENT_A:** the split between vendored, generated and locally written code is UNKNOWN.
13. **ERP_EXTENSION_A/B:** build and deploy path unknown (no pipeline), and which .NET code consumes their custom API pages is unknown.
14. **Workspace-level shared style folder:** one consuming app is confirmed; other consumers are UNKNOWN.
15. **Non-ASCII filename:** a theme file whose extension uses a Cyrillic look-alike character. It is invisible to extension globs; its content and export are commented out.

Other legacy notes:
- Two Angular generations are built with different package managers.
- The legacy UI carries old-browser shims.
- There are two CKEditor generations and two admin-theme generations (the jQuery and Angular editions of the same vendor family).
- The legacy product has roughly a decade of history.

A resolved ambiguity worth keeping for PKC design: directory-level MSBuild props declare a compile-exclusion glob for migration folders, but MSBuild evaluation shows it has **no effect**, so the migrations are compiled.

# PKC Risks

1. **Hidden runtime edges:** plugin modules are reached through reflection and a post-build copy, not project references. This risks false "unused" conclusions.
2. **False cross-application linking:** shared feature modules are compiled into multiple hosts across two product stacks. Attributing them to a single application would be wrong. Conversely, name similarity between the stacks must not be used to create edges.
3. **Mixed frontend generations:** legacy MVC plus jQuery and a newer Angular SPA serve the same product and hyperlink to each other. There are also two Angular majors in separate workspaces, and duplicate editor and theme generations.
4. **Vendor/custom JS intermixing:** first-party and vendor scripts share folders, a vendor tree contains a custom plugin, and bundles merge both. Libraries that look unused from views may be loaded via bundles.
5. **Bundles as runtime truth:** layouts load bundle outputs, not sources, so tracing back requires the bundler config.
6. **Misleading build configuration:** a compile-exclusion glob that has no effect. Scanners must use evaluated build items, not declared globs.
7. **Runtime-injected config:** UI-to-API edges derive from window globals or same-origin paths, so static confidence is MEDIUM at best.
8. **Stale/orphaned artefacts:** orphaned projects and tests, a stale workspace entry, stray files, stale pipeline triggers and a look-alike filename could all be mistaken for live code.
9. **Test contamination:** ~7 test projects, some hosting the full API in memory and referencing many modules, could inflate usage graphs if they are not kept as separate evidence.
10. **Large semantic scope and memory risk:**
    - The legacy UI (~8k files, ~6k of them static assets) and a single shared legacy layer (~2.7k files) dominate size.
    - Two vendor trees are ~1.4k files each, and the theme plugin pack is ~2.6k files.
    - Unbounded scans would be dominated by library code.
11. **Tracked generated files** (XML API docs, designer files, bundle outputs, migrations) will look like source unless they are classified.
12. **Documentation and AI-agent instruction files** exist in several areas. They describe intent, not behaviour, and should rank below code.
13. **Sensitive configuration:** config files contain endpoints and local defaults. PKC output should index key names only, never values.
14. **Non-.NET production code** (the ERP DSL) will be silently skipped by .NET/TS-only scanners.

# Recommended First Semantic Scan

Scope: roughly 1k C# files and ~850 TS files.

- **WEB_API_A:** DEEP. The composition root; establishes the module list, middleware, auth and routing conventions.
- **SHARED_MODULE_SET_A:** DEEP, all ~12 projects, excluding migration folders. The newest, explicitly wired and most active backend generation, shared by both product stacks.
- **Directly referenced SHARED_CORE_A:** TARGETED, followed only from SHARED_MODULE_SET_A references.
- **ANGULAR_UI_B:** DEEP, excluding specs and the template layout shell (LIGHT_INDEX). The most active UI, and it calls WEB_API_A.
- **SHARED_UI_LIB_A:** TARGETED.
- **LIGHT_INDEX:** the solution file, HOST_A, the frontend workspace config, and the CD pipelines for WEB_API_A, ANGULAR_UI_B and WEB_API_B.

Why this scope:
- It gives an end-to-end UI → API → module slice with no plugin-loading ambiguity.
- What is learned about the shared modules carries over later to the legacy stack.
- ANGULAR_UI_B's calls to WEB_API_B and its hyperlinks into MVC_UI_A should be **recorded as outbound edges, not followed**.

Not included initially:
- MVC_UI_A and all of its static assets. SHARED_CORE_B: record edges into it only.
- WEB_API_B/C/D, INTEGRATION_A/B, WORKER_A, SHARED_MODULE_L1, VENDORED_CLIENT_A.
- SHARED_MODULE_P1/P2 (second wave, together with ANGULAR_UI_A).
- WORKER_B, WORKER_C, GATEWAY_A.
- ANGULAR_UI_A, ANGULAR_UI_C, ANGULAR_UI_D.
- ERP_EXTENSION_A/B.
- All migrations, generated API docs and designer files.
- Tests (they may be attached later as evidence), tooling, and pipelines beyond the LIGHT_INDEX set.
- All third-party areas T1–T20.
