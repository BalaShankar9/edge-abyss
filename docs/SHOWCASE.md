# EdgeAbyss · project guide

A Unity riding game prototype with bike and horse controls, wind, stability and scoring systems.

**For:** Players who enjoy precision controls and developers interested in game systems.<br>
**Current stage:** Unity prototype · playable release pending<br>
**Reviewed:** 8 September 2026, from repository files and available GitHub workflow records. This is a source review, not a fresh application test or production certification.

## Start with the evidence

- [Assets/_Project](../Assets/_Project)
- [Packages/manifest.json](../Packages/manifest.json)
- [ProjectSettings/ProjectVersion.txt](../ProjectSettings/ProjectVersion.txt)

## A useful first demo

Record a short run showing rider selection, wind, a recoverable mistake, a crash and a restart. Include keyboard and gamepad controls in the release notes.

## Next release checklist

These are proposed acceptance gates. An unchecked item does not imply its implementation is absent; it means fresh release evidence is still needed.

- [ ] Verify the documented Unity version and a clean project import, then build for one target platform.
- [ ] Check reset, input and scoring behaviour and collect a small round of playtest feedback.
- [ ] Publish a playable tagged build, a short gameplay video and known limitations.

## What to measure

Playtest completion, restart success and recurring control issues, with session count disclosed.

Publish the dataset or evaluation method, date range, sample size and limitations with each result. Code size, feature counts and agent counts do not measure product usefulness.

## What a finished showcase contains

Playable build, gameplay recording and concise controls documentation.

Keep one dated release record containing the commit, setup steps, required services, checks run, known limitations and rollback instructions. Add screenshots from that version using fictional or consented data; identify demo fixtures clearly.

## Three ways to evaluate this project

| Visitor | Start here | Evidence to look for |
| --- | --- | --- |
| Potential client | The demo scenario above | A repeatable workflow and a measurable outcome |
| Engineering team | Linked source and tests | Design decisions, failure handling and reproducibility |
| Product user or collaborator | README setup and release notes | A supported journey, current limitations and feedback route |

[Repository overview](../README.md) · [Issues](https://github.com/BalaShankar9/edge-abyss/issues) · [More projects](https://github.com/BalaShankar9)
