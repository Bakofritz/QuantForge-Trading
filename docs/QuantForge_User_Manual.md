# QuantForge User Manual

## v26.36 cumulative release notes

QuantForge is a research, strategy-audit, simulation, optimization, replay, and controlled forward-testing platform. Live trading remains separately restricted.

### Strategy import and scrubbing
1. Place an imported strategy or indicator into quarantine.
2. Fingerprint the original source for provenance.
3. Run static feature/capability scrubbing.
4. Review the detected operation categories and component inventory.
5. Select or deselect components for the sanitized research representation.
6. Register the resulting sanitized artifact and provenance.
7. Run regression and authority-boundary validation.
8. Admit the strategy to historical/replay/simulated research only after required checks pass.

### Research domains
Imported strategies are designed to operate on historical data, simulated replay, and simulated-account real-time feeds by default. Live-account operation is a distinct authority domain and is not granted by research approval.

### Read-only multi-script optimization
Users may select one or multiple scripts for read-only analysis and optimization against approved market data. This research authority must remain unable to enter positions or change application settings.

### Data integrity
Research results should carry immutable identities for the dataset, strategy, execution policy, parameter set, and temporal partition. External links are provenance references; actual data bytes must be retrieved, fingerprinted, structurally validated, and admitted before being treated as an authoritative research dataset.

### Current documented data limitation
The latest documented MES minute-data validation identified 2,181,806 parsed rows across 25 NinjaTrader 8 `Last` exports. Structural checks reported zero malformed rows, duplicate timestamps, out-of-order timestamps, invalid OHLC relationships, negative-volume rows, or rows outside the 0.25-point price grid. Documented coverage gaps remain across June–September of 2023, 2024, and 2025, so a validated six-year performance claim remains blocked until missing source coverage is supplied.

### Build documentation
Build information is maintained cumulatively in the master build log rather than generating a separate documentation set for every build.

### Current planning progress
- Core research/simulation: ~75%
- Safety/governance: ~85%
- Strategy quarantine/scrubbing/import: ~80%
- Historical-data integrity/provenance: ~80%
- Read-only multi-strategy research: ~75%
- Optimization/research admission: ~70%
- Native Android/Windows production: ~45–50%
- Full end-to-end implementation: ~55–60%
- Overall usable research platform: ~70–75%

Percentages are engineering planning estimates, not independent runtime certification.
