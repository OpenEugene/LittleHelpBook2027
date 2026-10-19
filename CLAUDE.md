# Claude Instructions

## General Guidelines
- Avoid switching to the profiler or other agents unless clearly necessary. Explain the impact of such switches to prevent session loss and user frustration.

## Error Handling
- Controllers must catch service/data-layer exceptions, log them consistently at the controller boundary, and return stable HTTP status codes.
- Use concise controller error responses with `StatusCode` and `StatusCodes` constants instead of `Problem()`.

## Authorization
- Rely on Oqtane page/controller permissions and avoid additional authorization checks in service layer methods.

## Claude Code Behaviour
- Never run tools or agents in parallel. Always run one tool or agent at a time, sequentially. Prefer fewer tokens over faster execution.

## Project Skills
@.github/skills/oqtane-module-development/SKILL.md
@.github/skills/blazor-oqtane-js-interop/SKILL.md
@.github/skills/accessibility-and-validation/SKILL.md
@.github/skills/respect-working-tree/SKILL.md
@.github/skills/skiller/skill.md

## Documentation lives in `/docs` (Wikidown wiki)

- The `/docs` folder is a Wikidown wiki — structured markdown with `.order`
  navigation files. CLI commands (`wikidown read`, `wikidown write`, ...)
  address pages by title path: `/Getting-Started/Format`. Links written
  **inside page bodies** are different — see the wiki skill/subagent for the
  relative-link rule; do not use title-path links in markdown bodies, they
  404 on GitHub.
- A `wikidown-editor` subagent and a `wikidown` skill are configured for this
  repo. Use them for ANY read/write of `/docs/*.md`.
- Never edit `/docs/*.md` directly with `Write`/`Edit`. Use the `wikidown`
  CLI so `.order` files stay consistent.
- Prefer `wikidown edit` for small changes, `wikidown write-section` for one
  section, and `wikidown append` to add at the end; `wikidown write` is for
  full rewrites only and `wikidown new` for new pages.
- The wiki is the specification. `docs/Specifications/` pages describe what the
  application is meant to do and why, in plain language for people. They do not
  restate mechanics that can be read from the code (routes, class names, column
  types) and they contain no Gherkin or other test syntax.
- Keep the wiki in step with the intention of the code. When a change alters
  what the application does, update the relevant page as part of the same work
  by delegating to `wikidown-editor`. Do not leave it for a later pass.
