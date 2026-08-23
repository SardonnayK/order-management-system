# 0010 — Tailwind CSS v4 + spartan/ui as the frontend component library

Date: 2026-08-23

## Status

Accepted

## Context

The frontend needs a consistent set of basic UI building blocks (buttons, inputs, labels, modals, tables, cards) and a layout system without designing a component library from scratch. The desired look and working model is shadcn/ui — but shadcn/ui itself is React-only, so the Angular app needs an equivalent.

## Decision

Use **Tailwind CSS v4** for styling and **spartan/ui** as the component library — the shadcn/ui port for Angular, built on `@spartan-ng/brain` accessible primitives and the Angular CDK.

- spartan follows shadcn's copy-into-your-project model: the CLI (`ng g @spartan-ng/cli:ui <name>`) generates component source into `src/app/ui/`, owned and freely editable by us, imported via tsconfig path aliases as `@spartan-ng/helm/<name>`. There is no runtime component-library dependency to version-chase; only the headless `brain` primitives are a package.
- Tailwind v4 is wired through `@tailwindcss/postcss` (`.postcssrc.json`); `src/styles.css` declares the CSS layers, imports the spartan Tailwind preset, and defines the shadcn-style theme as CSS variables (light and `.dark` palettes), so theming is a variable swap, not component edits.
- Generated so far: button, input, label, dialog (modals), table, card, plus the shared `utils`. `components.json` configures the generator (target directory, style `vega`, import alias).
- There is no navbar primitive (same as shadcn); navigation bars are composed from Tailwind utilities and button variants.

## Consequences

- New components are one CLI command away and arrive as editable source, not a black box.
- The UI code in `src/app/ui/` is ours to maintain: upstream fixes arrive only by regenerating a component (overwriting local edits) — the standard shadcn trade-off.
- Tailwind v4 has no `tailwind.config.js`; content scanning and theme live in CSS (`@source`, variables), which tooling and documentation written for Tailwind v3 will not match.
- Components depend on `@spartan-ng/brain` and `@angular/cdk` staying compatible with the Angular major in use.
