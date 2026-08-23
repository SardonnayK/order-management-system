# 0001 — Record architecture decisions

Date: 2026-08-23

## Status

Accepted

## Context

This project is a full-stack .NET + Angular application built incrementally. Decisions about orchestration, configuration, and persistence were made during scaffolding, and their rationale lives in prompts and commit history where it is hard to find later.

## Decision

Record significant architecture decisions as numbered ADRs in `adr/` at the repository root, using the Nygard format (Context, Decision, Consequences). New decisions get a new file; superseded decisions are marked as such rather than edited.

## Consequences

- The "why" behind the setup survives beyond chat logs and commit messages.
- Reviewers of the assignment can follow the reasoning without reading the full history.
