---
name: work-item-loop
description: Inspect or validate the DedsiNative Markdown work-item queue without mutation, or execute/resume exactly one item through domain design, .NET backend, React admin frontend, verification, and status write-back. Use for docs/workItems queue inspection and the work-item development loop.
---

# Work Item Loop

Preview and validation modes are read-only. Execute and resume modes process exactly one work item per invocation; let the outer runner start another invocation.

## Prepare

1. Treat the directory containing `docs/`, `src/`, and `AGENTS.md` as the content root.
2. Read the content-root `AGENTS.md` and every more specific `AGENTS.md` governing files you may change.
3. Read [references/work-item-protocol.md](references/work-item-protocol.md) completely.
4. Inspect `git status --short`. Preserve unrelated and user-owned changes.
5. Determine the requested mode before selecting anything:
   - For preview, inspect, list, or validate requests, run the matching selector command, report its result, and stop without claiming or modifying any work item.
   - Only execute, continue, and resume requests may enter the claim and implementation workflow below.
6. Select the item specified by the caller. If none is specified for execution, run:

   ```bash
   node .agents/skills/work-item-loop/scripts/get-work-item.mjs \
     --work-items-path docs/workItems --mode next --json
   ```

7. Stop successfully when the result is `null`; report that the queue is empty.

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
   - Before backend or frontend coding, verify the work item's contract covers route, HTTP method, authentication, request and response fields, pagination when applicable, status codes, and error structure.
   - Record the accepted contract in the execution log. If it is incomplete, conflicting, or requires a material business decision, mark the item `blocked` and stop before implementation.
2. **Domain**
   - Read the work item and the matching document under `docs/domains`.
   - Resolve acceptance criteria into aggregates, invariants, value objects, domain events, repository contracts, and query contracts.
   - When the matching domain document is absent or needs a material change, explicitly load and follow `.agents/skills/create-domain-doc/SKILL.md`. Treat confirmed work-item facts as input; never invent unresolved business rules.
   - If a material business decision is missing, write the question and evidence to the execution log, mark the item `blocked`, and stop.
3. **Backend**
   - Route backend work through the module skills below. Treat every selected skill as explicitly invoked: read its `SKILL.md` completely, read every reference it requires, and follow its workflow and completion checks.
   - Select `.agents/skills/dedsi-add-dotnet-feature/SKILL.md` when the work item adds or changes a vertical business capability across Core, Infrastructure, or Host.
   - Also select `.agents/skills/dedsi-build-fastendpoint/SKILL.md` whenever an HTTP endpoint, request, response, validator, route, authentication rule, or status code changes.
   - Also select `.agents/skills/dedsi-efcore-persistence/SKILL.md` whenever an entity mapping, DbContext, repository, query implementation, persistence field, index, concurrency rule, or migration changes.
   - Read `.agents/rules/dotnet.md` before changing `src/dotnet`.
   - Before coding, state the selected backend skill names in the work-item execution log. Do not proceed with an applicable skill unread.
   - Delegate implementation to the project `backend` subagent. Give it the accepted contract, owned paths, selected skills, expected result, and validation commands. The main agent must not implement this stage directly or replace `backend` with another generic agent.
   - Require the subagent to implement in `src/dotnet` in this order: Core, Infrastructure, Endpoints, Host wiring, tests when present.
   - Do not bypass the domain layer from an endpoint.
   - Add a migration only when persistence shape changes and the required tooling/configuration is available.
   - If subagent delegation is unavailable, mark the item `blocked`, record the environment limitation, and stop.
4. **Frontend**
   - Route frontend work through the module skills below. Treat every selected skill as explicitly invoked: read its `SKILL.md` completely, read every reference it requires, and follow its workflow and completion checks.
   - Select `.agents/skills/dedsi-add-react-admin-feature/SKILL.md` for a complete page or business feature involving DTOs, service, page, route, or menu wiring.
   - Also select `.agents/skills/dedsi-build-react-admin-api/SKILL.md` whenever API DTOs, Axios calls, response contracts, pagination, or service exports change.
   - Also select `.agents/skills/dedsi-style-react-admin-ui/SKILL.md` whenever a page, component, layout, form, table, modal, responsive rule, or styling changes.
   - Read `.agents/rules/react-admin.md` before changing `src/react-admin`; also read `.agents/prompts/ui.md` for UI or styling changes.
   - Before coding, state the selected frontend skill names in the work-item execution log. Do not proceed with an applicable skill unread.
   - Delegate implementation to the project `frontend` subagent. Give it the same accepted contract, owned paths, selected skills, expected result, and validation commands. The main agent must not implement this stage directly or replace `frontend` with another generic agent.
   - Require the subagent to implement in `src/react-admin` in this order: typed DTOs, API service, pages/components, route/menu wiring.
   - Apply the centralized React rules under `.agents/rules/`.
   - Never introduce `any`.
   - If subagent delegation is unavailable, mark the item `blocked`, record the environment limitation, and stop.
5. **Verify**
   - Run the checks required by the work item and `AGENTS.md`.
   - At minimum run:

     ```bash
     dotnet build src/dotnet/DedsiNative.slnx
     cd src/react-admin && bun run build
     ```

   - Run focused tests or lint when relevant.
   - Reconcile backend and frontend against the accepted contract; if the contract changed, update the execution log before coordinating revisions.
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
