# User Stories — Ioan CV & Portfolio Site

Source: [PRD.md](../PRDs/PRD.md)

Stories are ordered by implementation phase and dependency. Local identifiers (ST-01, etc.) are for traceability; corresponding Jira issue keys are listed below.

## Jira Issue Keys

| Story | Jira | Story | Jira | Story | Jira |
|---|---|---|---|---|---|
| ST-00 | IOANCVAPP-24 |  |  |  |  |
| ST-01 | IOANCVAPP-10 | ST-08 | IOANCVAPP-15 | ST-15 | IOANCVAPP-23 |
| ST-02 | IOANCVAPP-5 | ST-09 | IOANCVAPP-11 | ST-16 | IOANCVAPP-21 |
| ST-03 | IOANCVAPP-6 | ST-10 | IOANCVAPP-16 | ST-17 | IOANCVAPP-18 |
| ST-04 | IOANCVAPP-7 | ST-11 | IOANCVAPP-13 | ST-18 | IOANCVAPP-20 |
| ST-05 | IOANCVAPP-8 | ST-12 | IOANCVAPP-12 | ST-19 | IOANCVAPP-19 |
| ST-06 | IOANCVAPP-9 | ST-13 | IOANCVAPP-14 |  |  |
| ST-07 | IOANCVAPP-17 | ST-14 | IOANCVAPP-22 |  |  |

## Phase 0 — GitHub Repository Setup

## ST-00 — Set up the GitHub repository

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Small  
**Phase**: 0 — GitHub Repository Setup  
**Labels**: repository, setup

### Description
As a developer, I want an initialized GitHub repository for the project, so that implementation work has a shared, version-controlled home from the first step.

### Acceptance Criteria
- [ ] Given the project repository is created, when its default branch is inspected, then `main` exists and the repository can be cloned.
- [ ] Given a clean checkout, when the repository is inspected, then it contains a concise README describing the project and intended solution structure, plus a `.gitignore` covering .NET, Node, IDE files, local databases, and local secrets.
- [ ] Given the initial project files are committed, when the repository is checked, then no credentials, tokens, or other secrets are present.
- [ ] Given a developer follows the README, when they clone the repository, then the documented project structure is available as the starting point for ST-01.

### Technical Notes
- Keep automated build/test/deployment workflows out of this ticket; they are covered by ST-18.
- Do not commit generated build artifacts, local SQLite databases, or developer-specific secret/configuration files.

### Dependencies
- Blocked by: None
- Blocks: ST-01

## Phase 1 — Backend Foundation

## ST-01 — Establish the backend domain and SQLite persistence

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — Backend Foundation  
**Labels**: backend, database

### Description
As a developer, I want the API, domain, and infrastructure projects to share a minimal EF Core SQLite data model, so that portfolio content has a persistent source of truth in development and production.

### Acceptance Criteria
- [ ] Given a clean checkout, when the backend is built, then the API, Domain, and Infrastructure projects compile with the agreed .NET and EF Core versions.
- [ ] Given the initial schema, when a migration is applied to an empty SQLite database, then Profile, ExperienceEntry, and Project records can be stored and read.
- [ ] Given local development configuration, when the API starts, then it uses SQLite without requiring a paid or external database service.
- [ ] Given content entities, when they are exposed at API boundaries, then explicit DTOs are used rather than serializing entities directly.

### Technical Notes
- Follow the indicative `/backend/src/IoanCvApp.Api`, `IoanCvApp.Domain`, and `IoanCvApp.Infrastructure` structure in PRD Section 6.
- Keep fields minimal and text-forward; represent tech stacks as simple string lists.
- Add the initial EF Core migration and keep secrets out of committed configuration.

### Dependencies
- Blocked by: None
- Blocks: ST-02, ST-03, ST-04, ST-05, ST-06, ST-17

## ST-02 — Serve public profile, experience, and project data

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 1 — Backend Foundation  
**Labels**: backend, api

### Description
As a public visitor, I want anonymous read-only API endpoints for profile, experience, and projects, so that the portfolio can display current database-backed content without requiring a login.

### Acceptance Criteria
- [ ] Given profile content exists, when an unauthenticated client requests `GET /api/public/profile`, then the API returns the profile DTO.
- [ ] Given experience entries exist, when an unauthenticated client requests `GET /api/public/experience`, then the API returns entries in the defined display order.
- [ ] Given project entries exist, when an unauthenticated client requests `GET /api/public/projects`, then the API returns project DTOs including tech stack and optional image URL.
- [ ] Given no matching content exists, when a public endpoint is requested, then the API returns a valid empty result or documented not-found response without an unhandled exception.
- [ ] Given a development environment, when a developer opens Swagger, then the public endpoints and DTO contracts are documented and callable.

### Technical Notes
- Use the `/api/public/*` route group and DTOs as specified in PRD Sections 6 and 10.
- Seed non-sensitive sample content only if needed for local validation; never seed real admin credentials here.

### Dependencies
- Blocked by: ST-01
- Blocks: ST-07, ST-08, ST-09, ST-10, ST-11

## Phase 2 — Admin Auth & CRUD

## ST-03 — Authenticate the single site administrator

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 2 — Admin Auth & CRUD  
**Labels**: backend, auth, security

### Description
As the site owner, I want to authenticate with a seeded admin account and receive a short-lived JWT, so that only I can access content-changing operations.

### Acceptance Criteria
- [ ] Given valid configured admin credentials, when I submit them to `POST /api/auth/login`, then the API returns a signed JWT and UTC expiry.
- [ ] Given invalid credentials, when I attempt login, then the API returns `401` with a structured error and does not disclose which credential was incorrect.
- [ ] Given an admin password, when the user is seeded, then only a password hash is persisted and no plaintext password is logged or committed.
- [ ] Given a JWT, when the API validates it, then signature, issuer/audience as configured, expiry, and required admin claim are checked.
- [ ] Given repeated login attempts, when the configured rate limit is exceeded, then further requests are throttled according to the documented policy.

### Technical Notes
- Configure the signing key and seed credentials via user-secrets/local ignored settings or deployment settings.
- Use ASP.NET Core Identity password hashing or an equivalent established password hasher; do not implement custom hashing.
- Configure HTTPS and restrict CORS to the frontend origin plus localhost development origins.

### Dependencies
- Blocked by: ST-01
- Blocks: ST-04, ST-05, ST-06, ST-07, ST-12, ST-17

## ST-04 — Manage experience entries through protected API endpoints

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 2 — Admin Auth & CRUD  
**Labels**: backend, api, experience

### Description
As the site owner, I want authenticated endpoints to create, edit, and delete experience entries, so that I can keep my work history current without changing code or redeploying.

### Acceptance Criteria
- [ ] Given a valid admin JWT and valid input, when I POST an experience entry, then it is persisted and the created representation is returned.
- [ ] Given a valid admin JWT and an existing entry, when I PUT updated fields, then the persisted entry and public API representation reflect the changes.
- [ ] Given a valid admin JWT and an existing entry, when I DELETE it, then it is no longer returned by the public experience endpoint.
- [ ] Given a missing or invalid JWT, when any experience write endpoint is called, then the API responds with `401 Unauthorized`.
- [ ] Given invalid dates or required fields, when a write request is submitted, then the API returns a structured validation response and makes no partial change.

### Technical Notes
- Implement `/api/admin/experience` routes behind authorization; include ordering/reordering support consistent with the public display order.
- Keep controllers thin and delegate persistence/validation to services.

### Dependencies
- Blocked by: ST-01, ST-03
- Blocks: ST-07, ST-14

## ST-05 — Manage project entries through protected API endpoints

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 2 — Admin Auth & CRUD  
**Labels**: backend, api, projects

### Description
As the site owner, I want authenticated endpoints to create, edit, and delete project entries, so that I can maintain my portfolio without a code change.

### Acceptance Criteria
- [ ] Given a valid admin JWT and valid input, when I POST a project, then its name, description, tech stack, links, and optional image URL are persisted.
- [ ] Given a valid admin JWT and an existing project, when I PUT changes, then the public projects endpoint returns the updated data.
- [ ] Given a valid admin JWT and an existing project, when I DELETE it, then it is absent from the public projects endpoint.
- [ ] Given no valid JWT, when a project write endpoint is called, then the API responds with `401 Unauthorized`.
- [ ] Given invalid or missing required fields, when a project is submitted, then the API returns structured validation errors without persisting invalid data.

### Technical Notes
- Implement `/api/admin/projects` routes and request/response DTOs.
- The MVP may accept an image URL; image upload is not required.

### Dependencies
- Blocked by: ST-01, ST-03
- Blocks: ST-07, ST-15

## ST-06 — Update profile and CV content through a protected API endpoint

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Small  
**Phase**: 2 — Admin Auth & CRUD  
**Labels**: backend, api, profile

### Description
As the site owner, I want to update my profile content through an authenticated endpoint, so that I can keep my About/CV information accurate without redeploying the site.

### Acceptance Criteria
- [ ] Given a valid admin JWT and valid profile fields, when I PUT to `/api/admin/profile`, then the single profile record is updated.
- [ ] Given a successful update, when a public client reads `/api/public/profile`, then it receives the saved values.
- [ ] Given a missing or invalid JWT, when the profile endpoint is updated, then the API responds with `401 Unauthorized`.
- [ ] Given invalid profile data, when an update is submitted, then the API returns structured validation errors and retains the prior profile.

### Technical Notes
- Support the About content, skills, and contact links described in the PRD.
- Treat the profile as a single record with a defined initialization/empty state.

### Dependencies
- Blocked by: ST-01, ST-03
- Blocks: ST-07, ST-13

## ST-07 — Verify backend authorization, validation, and CRUD behavior

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 2 — Admin Auth & CRUD  
**Labels**: backend, testing, security

### Description
As a maintainer, I want automated API tests for public reads, authentication, and admin writes, so that security boundaries and core data workflows remain reliable as the application changes.

### Acceptance Criteria
- [ ] Given the API test suite runs, when unauthenticated requests target every admin write route, then each is verified to return `401`.
- [ ] Given valid and invalid login attempts, when auth tests run, then successful token issuance and rejection behavior are covered.
- [ ] Given CRUD requests for Profile, Experience, and Projects, when tests run against an isolated SQLite test database, then persistence and public visibility are verified.
- [ ] Given invalid input and missing record cases, when tests run, then structured errors and no-partial-write behavior are verified.
- [ ] Given the backend CI build, when tests run, then failures fail the pipeline with actionable output.

### Technical Notes
- Follow the PRD's `/backend/tests/IoanCvApp.Api.Tests` structure.
- Avoid using production or developer databases in tests.

### Dependencies
- Blocked by: ST-02, ST-03, ST-04, ST-05, ST-06
- Blocks: ST-18

## Phase 3 — React Frontend

## ST-08 — Establish the React application and typed API client

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 3 — React Frontend  
**Labels**: frontend, infrastructure

### Description
As a developer, I want a React, TypeScript, and Vite frontend with routing and a typed API client, so that public and admin experiences can use consistent contracts with the backend.

### Acceptance Criteria
- [ ] Given a clean checkout, when frontend dependencies are installed and the app is built, then the TypeScript/Vite build completes successfully.
- [ ] Given frontend routes, when a visitor opens the application, then About, Experience, Projects, login, and admin route destinations are registered.
- [ ] Given the API base URL is configured per environment, when the API client makes a request, then it targets that URL without hardcoded production credentials or secrets.
- [ ] Given API errors or loading states, when a page requests data, then the client can expose typed results and explicit failure states to the UI.

### Technical Notes
- Use React 18, TypeScript, Vite, React Router, and Tailwind CSS as specified by the PRD.
- Keep typed frontend models aligned with backend DTOs.

### Dependencies
- Blocked by: ST-02
- Blocks: ST-09, ST-10, ST-11, ST-12

## ST-09 — Present the About page, resume download, and AI-build story

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Small  
**Phase**: 3 — React Frontend  
**Labels**: frontend, profile

### Description
As a recruiter or technical visitor, I want a concise About page with Ioan's profile, contact links, resume download, and explanation of the AI-assisted build, so that I can quickly understand his background and how this portfolio was made.

### Acceptance Criteria
- [ ] Given profile data is available, when I open the About page, then name, headline, bio, skills, and contact links are displayed from the public API.
- [ ] Given the resume PDF asset is available, when I activate “Download Resume”, then the PDF downloads or opens successfully.
- [ ] Given I view the About page, when I reach the “Built with AI” section, then static copy identifies the ASP.NET Core and React stack and explains the AI-assisted process without implying AI-generated portfolio features.
- [ ] Given I view the “Built with AI” section, when I activate the “View source on GitHub” link, then the project's GitHub repository opens in a new tab with `rel="noopener noreferrer"`, and the link is keyboard accessible.
- [ ] Given profile loading or API failure, when the page renders, then an explicit loading or recoverable error state is shown instead of a broken/empty page.

### Technical Notes
- The “Built with AI” content is static frontend content per the PRD, not admin-editable.
- Resume asset path should be deployment-safe and the link should remain keyboard accessible.
- The GitHub repository URL (https://github.com/ioanseverin/IoanCVApp, created in ST-00) is static frontend configuration, not admin-editable or database-backed.

### Dependencies
- Blocked by: ST-02, ST-08
- Blocks: ST-16

## ST-10 — Present the experience timeline

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Small  
**Phase**: 3 — React Frontend  
**Labels**: frontend, experience

### Description
As a recruiter, I want to scan a reverse-chronological experience timeline, so that I can assess Ioan's career fit quickly without needing to open a PDF.

### Acceptance Criteria
- [ ] Given experience entries exist, when I open the Experience page, then roles show title, company, dates, description, and relevant tech tags.
- [ ] Given roles have different dates, when the list is displayed, then entries follow reverse chronological order.
- [ ] Given an end date is absent, when a current role is displayed, then its end date is rendered as “Present”.
- [ ] Given the API is empty or unavailable, when the page loads, then a clear empty or error state is displayed.

### Technical Notes
- Consume `GET /api/public/experience`; do not hardcode career entries in components.

### Dependencies
- Blocked by: ST-02, ST-08
- Blocks: ST-16

## ST-11 — Present the project portfolio

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Small  
**Phase**: 3 — React Frontend  
**Labels**: frontend, projects

### Description
As a recruiter or collaborator, I want to browse project cards with descriptions, technology tags, and relevant links, so that I can evaluate the breadth and type of Ioan's work.

### Acceptance Criteria
- [ ] Given project entries exist, when I open the Projects page, then each card displays its name, description, and tech stack.
- [ ] Given an image URL or external project link is available, when a card is displayed, then the image/link is shown and usable; absent optional fields do not break the card.
- [ ] Given the API is empty or unavailable, when the page loads, then a clear empty or error state is displayed.
- [ ] Given project data changes through the admin API, when the page is refreshed, then the updated content is visible without a frontend redeploy.

### Technical Notes
- Consume `GET /api/public/projects`; keep content database-backed.

### Dependencies
- Blocked by: ST-02, ST-08
- Blocks: ST-16

## ST-12 — Log in to the protected admin area

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 3 — React Frontend  
**Labels**: frontend, auth, security

### Description
As the site owner, I want to log in and access protected admin routes, so that I can manage content through the site while visitors remain read-only.

### Acceptance Criteria
- [ ] Given I submit valid credentials, when login succeeds, then the frontend stores the returned token in memory or the selected secure client storage and navigates to the admin area.
- [ ] Given invalid credentials, when login fails, then an understandable error is shown without exposing sensitive details.
- [ ] Given I open an admin route without a valid stored session, when route protection runs, then I am redirected to login.
- [ ] Given my token expires or an admin API call returns `401`, when the frontend handles the response, then the session is cleared and I am prompted to log in again.
- [ ] Given I log out, when logout completes, then the token is removed and protected routes are no longer accessible.

### Technical Notes
- Use the backend login endpoint and attach `Authorization: Bearer` only to admin API requests.
- Do not persist credentials; there is no refresh-token flow in the MVP.

### Dependencies
- Blocked by: ST-03, ST-08
- Blocks: ST-13, ST-14, ST-15

## ST-13 — Edit profile content in the admin UI

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Small  
**Phase**: 3 — React Frontend  
**Labels**: frontend, profile, admin

### Description
As the site owner, I want an authenticated form for my About/CV profile, so that I can update my bio, skills, and contact links without a code deployment.

### Acceptance Criteria
- [ ] Given I am authenticated, when I open profile administration, then current profile values are loaded into an editable form.
- [ ] Given I submit valid changes, when the save succeeds, then a success state is shown and the public About page reflects the updated values.
- [ ] Given invalid values or an API failure, when saving, then field or request errors are displayed and unsaved changes are not falsely reported as saved.
- [ ] Given I am not authenticated, when I attempt to open or submit the profile editor, then the protected route/API denies access.

### Technical Notes
- Limit user effort to the PRD goal of roughly 2–3 form interactions per change where practical.

### Dependencies
- Blocked by: ST-06, ST-12
- Blocks: ST-16

## ST-14 — Manage experience entries in the admin UI

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 3 — React Frontend  
**Labels**: frontend, experience, admin

### Description
As the site owner, I want to create, edit, delete, and order experience entries through the admin UI, so that the public career timeline stays accurate without code changes.

### Acceptance Criteria
- [ ] Given I am authenticated, when I open experience administration, then existing entries and their current order are shown.
- [ ] Given valid entry details, when I create or edit an entry, then the API saves it and the UI reports success.
- [ ] Given I confirm deletion of an entry, when deletion succeeds, then it is removed from the admin list and public experience results.
- [ ] Given I change display order, when the change is saved, then the public timeline uses the new order.
- [ ] Given invalid input or an API error, when I submit changes, then errors are surfaced and the entry is not shown as saved.

### Technical Notes
- Keep forms aligned with API DTO validation and support “Present” for a missing end date.

### Dependencies
- Blocked by: ST-04, ST-12
- Blocks: ST-16

## ST-15 — Manage project entries in the admin UI

**Type**: Feature  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 3 — React Frontend  
**Labels**: frontend, projects, admin

### Description
As the site owner, I want to create, edit, and delete project entries from the admin UI, so that the portfolio can be kept current without changing code or redeploying.

### Acceptance Criteria
- [ ] Given I am authenticated, when I open project administration, then existing projects are shown with their editable fields.
- [ ] Given valid name, description, tech stack, and optional links/image URL, when I create or update a project, then it is persisted and success is reported.
- [ ] Given I confirm deletion, when the delete succeeds, then the project no longer appears in admin or public results.
- [ ] Given invalid input or an API failure, when I save, then errors are clearly shown and unsaved values are not reported as persisted.
- [ ] Given I am not authenticated, when I attempt a project write, then access is denied.

### Technical Notes
- Use an image URL field for MVP; upload infrastructure is not required.

### Dependencies
- Blocked by: ST-05, ST-12
- Blocks: ST-16

## ST-16 — Make the public and admin experiences responsive

**Type**: Enhancement  
**Jira Type**: Story  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 3 — React Frontend  
**Labels**: frontend, responsive, accessibility

### Description
As a mobile visitor or administrator, I want the site and its forms to work at phone and desktop widths, so that I can review or maintain the portfolio from any device.

### Acceptance Criteria
- [ ] Given a narrow mobile viewport, when I browse public pages, then content reflows without horizontal scrolling and navigation collapses into a usable menu.
- [ ] Given mobile or desktop navigation, when I use keyboard or touch, then links and menu controls remain operable with appropriately sized targets.
- [ ] Given admin forms at mobile widths, when I edit content, then fields and save/cancel actions remain visible and usable.
- [ ] Given representative desktop and mobile viewport widths, when layout tests/manual checks run, then typography, cards, and navigation remain readable and do not overlap.

### Technical Notes
- Apply the responsive pass across About, Experience, Projects, login, and admin screens, not only the landing page.

### Dependencies
- Blocked by: ST-09, ST-10, ST-11, ST-13, ST-14, ST-15
- Blocks: ST-18

## Phase 4 — Azure Deployment & CI/CD

## ST-17 — Configure the production Azure environment and persistent SQLite

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 4 — Azure Deployment & CI/CD  
**Labels**: deployment, azure, database

### Description
As the site owner, I want the API, frontend, secrets, and SQLite database configured on the agreed free Azure tiers, so that the MVP is publicly reachable without recurring hosting cost.

### Acceptance Criteria
- [ ] Given the approved Azure free-tier services, when the environment is provisioned, then the API runs on App Service F1 and the frontend is hosted on Static Web Apps Free.
- [ ] Given production configuration, when the API starts, then JWT settings, admin seed credentials, frontend origin, and SQLite file location are read from deployment settings rather than committed files.
- [ ] Given SQLite writes in production, when the app is restarted or redeployed, then data remains in the configured persistent `/home` storage.
- [ ] Given production deployment, when migrations are released, then migrations are applied through an explicit release step rather than automatically at application startup.
- [ ] Given cross-origin frontend requests, when the browser calls the API, then only configured production and approved development origins are allowed over HTTPS.
- [ ] Given known Free F1 constraints, when the environment is handed over, then cold-start, quota, and single-instance SQLite tradeoffs are documented.

### Technical Notes
- Keep the SQLite deployment single-instance; use `WEBSITES_ENABLE_APP_SERVICE_STORAGE=true` and verify the actual persistent path.
- Do not move to paid tiers or Azure SQL without a separate explicit decision.

### Dependencies
- Blocked by: ST-01, ST-03
- Blocks: ST-18

## ST-18 — Automate build, test, and deployment with GitHub Actions

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Medium  
**Phase**: 4 — Azure Deployment & CI/CD  
**Labels**: deployment, ci-cd, testing

### Description
As the site owner, I want automated backend/frontend validation and deployment on pushes to `main`, so that approved changes can be shipped consistently to the intended Azure services.

### Acceptance Criteria
- [ ] Given a push to `main`, when GitHub Actions runs, then backend build/tests and frontend build complete before deployment proceeds.
- [ ] Given deployment credentials and secrets, when workflows execute, then credentials are stored as protected GitHub/Azure settings and never printed or committed.
- [ ] Given successful builds, when the workflow deploys, then API and frontend are released to their intended Azure services.
- [ ] Given a failed test or deployment, when the workflow completes, then the run fails visibly and does not report a successful release.

### Technical Notes
- Keep deployment credentials in protected workflow settings; do not commit them to the repository.

### Dependencies
- Blocked by: ST-07, ST-16, ST-17
- Blocks: ST-19

## ST-19 — Verify the deployed MVP with production smoke tests

**Type**: Technical  
**Jira Type**: Task  
**Priority**: High  
**Complexity**: Small  
**Phase**: 4 — Azure Deployment & CI/CD  
**Labels**: deployment, testing, azure

### Description
As the site owner, I want to verify the deployed portfolio and its persistent content workflows, so that I know the MVP is publicly usable and admin changes survive an application restart.

### Acceptance Criteria
- [ ] Given the production URL, when smoke tests run, then public About, Experience, and Projects content loads over HTTPS.
- [ ] Given valid administrator credentials, when I log in to production, then the admin session works and protected write endpoints reject unauthenticated requests.
- [ ] Given an authenticated production content edit, when the API/database are restarted, then the change remains visible on the public site.
- [ ] Given frontend and API are hosted separately, when browser requests are checked, then origin configuration and CORS work for the deployed frontend without allowing unapproved origins.
- [ ] Given the selected service configuration, when the release is signed off, then it uses only the approved free tiers and the expected $0/month hosting assumption has been checked.

### Technical Notes
- Run checks against production or a staging configuration with equivalent origin/storage settings before declaring MVP complete.
- Record known free-tier cold-start, quota, and persistence limitations in the deployment handover.

### Dependencies
- Blocked by: ST-18
- Blocks: None

## Traceability and validation

- Public About/profile, resume download, and static AI-build section: ST-09.
- Public Experience and Projects: ST-10, ST-11.
- Admin authentication and server-side write protection: ST-03, ST-12, ST-04, ST-05, ST-06.
- Admin content management: ST-13, ST-14, ST-15.
- Responsive desktop/mobile behavior: ST-16.
- Backend persistence, API contracts, validation, and tests: ST-01, ST-02, ST-04–ST-07.
- Azure free-tier deployment, persistent data, CI/CD, and production verification: ST-17–ST-19.
- Deferred AI job-fit generation and internationalization are intentionally excluded because the PRD marks them out of scope.

All dependencies point to earlier foundational work or one final delivery task; the dependency graph is acyclic. Acceptance criteria are observable and testable. ST-18 spans multiple deployment concerns and may need subdivision if implementation exceeds one working day.
