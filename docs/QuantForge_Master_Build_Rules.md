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

## v26.36 planning status

These are engineering planning estimates, not independently measured runtime test results:
- Core research/simulation: ~75%
- Safety/governance: ~85%
- Strategy quarantine/scrubbing/import: ~80%
- Historical-data integrity/provenance: ~80%
- Read-only multi-strategy research: ~75%
- Optimization/research admission: ~70%
- Native Android/Windows production: ~45–50%
- Full end-to-end implementation: ~55–60%
- Overall usable research platform: ~70–75%
