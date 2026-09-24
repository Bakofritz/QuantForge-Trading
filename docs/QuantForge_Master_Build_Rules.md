# QuantForge Master Build Rules

## v26.36 cumulative rules
- Continue stable master-build iterations as far as practical and report milestone and overall progress percentages.
- Append build documentation to cumulative running files rather than creating a new documentation set for every build.
- Include a detailed user manual with each release/build.
- Imported and scrubbed strategies/indicators default to historical data, simulated replay, and simulated-account real-time feeds.
- Live-account operation remains separately restricted and is never implicitly granted by successful scrubbing, import, simulation, replay, or optimization approval.
- Strategy Auditor and Feature Scrubber operate as a first-line quarantine/import safety layer. Detected operation categories are inventoried and the user can select or deselect components before sanitized research import and authority registration.
- Read-only multi-script optimization may analyze and optimize one or many scripts against read-only market data without position entry or application-setting changes.
- GitHub is the official QuantForge master-build source repository when connected GitHub tooling permits writes. Never claim a write that was not confirmed.
- Safety and research boundaries are fail-closed: research authority does not become live trading authority.

## v26.37–v26.39 additions
- Establish architecture contracts before executable implementation where the repository lacks a validated build system.
- Treat data admission as upstream of every research execution path.
- Treat strategy admission as quarantine → fingerprint → scrub → feature inventory → authority classification → user selection → sanitized adapter → regression validation → provenance registration.
- Require isolated research jobs with explicit authority domains.
- Bind publishable results to immutable dataset, strategy, execution policy, parameter set, temporal partition, and job identities.
- Never represent documentation or architectural contracts as proof of compiled production functionality.

## v26.37–v26.39 planning status
These are engineering planning estimates, not independently measured runtime test results:
- Core research/simulation: ~76%
- Safety/governance: ~87%
- Strategy quarantine/scrubbing/import: ~82%
- Historical-data integrity/provenance: ~82%
- Read-only multi-strategy research: ~77%
- Optimization/research admission: ~72%
- Native Android/Windows production: ~45–50%
- Full end-to-end implementation: ~57–61%
- Overall usable research platform: ~72–76%

## v26.49+ approval and queue-control rule
- When a build iteration requires user approval before a repository-changing commit, the final statement of the GPT build-status message must be exactly: "Does this build iteration have your approval to commit?"
- The approval question must be the last statement in that message; no additional instructions, questions, or build-status statements may follow it.
- If nothing is currently queued for approval, the final statement of the GPT build-status message must clearly state that no build iteration is currently queued for approval.
- No approval-gated commit, GitHub file/folder structural change, merge, or equivalent repository mutation may occur before the required user approval.
- A user approval applies only to the exact build iteration and planned repository changes presented immediately before the approval request. Material changes require a new approval request.
- "APPROVED" or equivalent explicit approval authorizes the exact queued operation; it does not authorize unrelated or materially changed repository work.
