---
name: work-item-loop
description: Drive one DedsiNative Markdown work item through domain design, .NET backend, React admin frontend, verification, and status write-back. Use when asked to execute, continue, resume, validate, or inspect the next work item or the work-item development loop under docs/workItems.
---

# Work Item Loop

Process exactly one work item per invocation. Let the outer runner start another invocation.

## Prepare

1. Treat the directory containing `docs/`, `src/`, and `AGENTS.md` as the content root.
2. Read the content-root `AGENTS.md` and every more specific `AGENTS.md` governing files you may change.
3. Read [references/work-item-protocol.md](references/work-item-protocol.md) completely.
4. Inspect `git status --short`. Preserve unrelated and user-owned changes.
5. Select the item specified by the caller. If none is specified, run:

   ```powershell
   pwsh -NoProfile -File .agents/skills/work-item-loop/scripts/Get-WorkItem.ps1 `
     -WorkItemsPath docs/workItems -Mode Next -Json
   ```

6. Stop successfully when the result is `null`; report that the queue is empty.

## Claim or resume

- Require the selected item to be `ready`, `failed`, or `in-progress`.
- Before implementation, set its status to `in-progress`, set the current stage, increment `work-item-attempt` for `ready` or `failed`, and update `work-item-updated-at`.
- Resume an `in-progress` item without incrementing its attempt.
- Never process a `draft`, `completed`, `blocked`, or `cancelled` item.
- Never select or modify a second work item.

## Execute the stages

Follow these gates in order:

1. **Work-item analysis**
   - Parse the goal, business rules, scope, exclusions, and every acceptance criterion.
   - Decide whether the backend and frontend stages apply; record any non-applicable stage and its reason.
   - Identify the module skills required by the routing rules below before changing code.
2. **Domain**
   - Read the work item and the matching document under `docs/domains`.
   - Resolve acceptance criteria into aggregates, invariants, value objects, domain events, repository contracts, and query contracts.
   - Create or update the domain Markdown document when the design changes.
   - If a material business decision is missing, write the question and evidence to the execution log, mark the item `blocked`, and stop.
3. **Backend**
   - Route backend work through the module skills below. Treat every selected skill as explicitly invoked: read its `SKILL.md` completely, read every reference it requires, and follow its workflow and completion checks.
   - Select `src/dotnet/.agents/skills/dedsi-add-dotnet-feature/SKILL.md` when the work item adds or changes a vertical business capability across Core, Infrastructure, or Host.
   - Also select `src/dotnet/.agents/skills/dedsi-build-fastendpoint/SKILL.md` whenever an HTTP endpoint, request, response, validator, route, authentication rule, or status code changes.
   - Also select `src/dotnet/.agents/skills/dedsi-efcore-persistence/SKILL.md` whenever an entity mapping, DbContext, repository, query implementation, persistence field, index, concurrency rule, or migration changes.
   - Before coding, state the selected backend skill names in the work-item execution log. Do not proceed with an applicable skill unread.
   - Implement in `src/dotnet` in this order: Core, Infrastructure, Host/FastEndpoints, tests when present.
   - Do not bypass the domain layer from an endpoint.
   - Add a migration only when persistence shape changes and the required tooling/configuration is available.
4. **Frontend**
   - Route frontend work through the module skills below. Treat every selected skill as explicitly invoked: read its `SKILL.md` completely, read every reference it requires, and follow its workflow and completion checks.
   - Select `src/react-admin/.agents/skills/dedsi-add-react-admin-feature/SKILL.md` for a complete page or business feature involving DTOs, service, page, route, or menu wiring.
   - Also select `src/react-admin/.agents/skills/dedsi-build-react-admin-api/SKILL.md` whenever API DTOs, Axios calls, response contracts, pagination, or service exports change.
   - Also select `src/react-admin/.agents/skills/dedsi-style-react-admin-ui/SKILL.md` whenever a page, component, layout, form, table, modal, responsive rule, or styling changes.
   - Before coding, state the selected frontend skill names in the work-item execution log. Do not proceed with an applicable skill unread.
   - Implement in `src/react-admin` in this order: typed DTOs, API service, pages/components, route/menu wiring.
   - Read and apply the nested React `AGENTS.md`.
   - Never introduce `any`.
5. **Verify**
   - Run the checks required by the work item and `AGENTS.md`.
   - At minimum run:

     ```powershell
     dotnet build src/dotnet/DedsiNative.slnx
     Push-Location src/react-admin; bun run build; Pop-Location
     ```

   - Run focused tests or lint when relevant.
   - Review the diff for scope drift and verify every acceptance criterion with concrete evidence.

Update `work-item-stage` as each implementation stage begins. The work-item analysis uses `backlog`; domain, backend, frontend, and verification use their matching stage values.

## Close the invocation

- Mark `completed` and stage `done` only when every acceptance criterion passes and required builds/tests succeed.
- Mark `failed` and retain the failing stage when implementation or verification fails but another attempt can reasonably fix it.
- Mark `blocked` when progress requires a business decision, unavailable authority, destructive migration decision, secret, or external state.
- Append a timestamped Markdown entry inside `LOOP_LOG_START` / `LOOP_LOG_END` with changed paths, commands, results, and remaining issues.
- Update `work-item-updated-at` on every terminal transition.
- Do not commit, push, create a PR, delete migrations, reset Git, or overwrite unrelated changes unless the user explicitly authorizes it.
- End with the work-item ID, terminal status, verification evidence, and blocker or next action.
