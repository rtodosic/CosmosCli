# Copilot instructions for CosmosCli

## Project purpose

This repository contains CosmosCli, a .NET command-line tool for working with Azure Cosmos DB for NoSQL. The tool is centered around a simple workflow: select JSON documents, optionally transform them, and then upsert or delete them back into Cosmos DB.

Keep the CLI behavior, command names, config precedence, and output conventions consistent with the existing tool and documentation.

## Repository layout

- `CosmosCli/` – main application and command implementation.
- `Commands/` – CLI command classes and command handlers.
- `Parameters/` – command-line option and argument models.
- `Extensions/` – extension helpers used across the app.
- `Validations/` – validation logic for arguments, config, and user input.
- `Utilities/` – shared helper logic used by commands and validation.
- `README.md` – public usage documentation and examples.
- `CosmosCli.sln` – solution entry point.

## Build and validation

Use the repository's configured .NET SDK and prefer the smallest relevant validation command.

```bash
dotnet build CosmosCli.sln
```

For a focused CLI smoke check during development:

```bash
dotnet run --project CosmosCli -- --help
```

If tests are present or added, use the smallest relevant `dotnet test` command that validates the changed behavior instead of a broad suite.

## Coding expectations

- Keep changes surgical and aligned to the existing architecture.
- Follow the current command and option naming patterns already used by the project.
- Respect the existing `Cocona.Lite` command structure and CLI conventions.
- Preserve public CLI behavior unless a task explicitly requires a breaking change.
- Respect the project's nullable and .NET conventions.
- Prefer reusing helper methods and validation patterns already in `Utilities` and `Validations` instead of introducing new ad hoc patterns.
- Keep user-facing output human-readable and compatible with the current README examples.

## Cosmos-specific guidance

- Respect environment-variable and config-file precedence already documented in the README.
- Preserve the `cosmos` tool command name and existing subcommand semantics.
- Keep `config.json` behavior and connection settings consistent with the rest of the project.
- Avoid introducing broad refactors or unrelated cleanup while fixing a specific issue.

## Documentation and compatibility

If a change affects command syntax, option names, output shape, config keys, or command behavior, validate whether the README or usage examples also need to be updated. Prefer minimal, accurate documentation updates over unrelated edits.

## Commit messages and versioning

- Use Conventional Commits for all commit messages, in the format `type(scope): subject` (scope is optional), for example `feat(container): support item patching` or `docs: clarify config precedence`.
- Use a meaningful type from the repository's commit linter: `build`, `feat`, `ci`, `chore`, `docs`, `fix`, `perf`, `refactor`, `revert`, `style`, or `test`.
- Mark breaking changes with `!` before the colon, such as `feat!: change config file lookup`.
- Keep the first line within the repository linter's 90-character limit. The Husky commit hook validates the message.
- Conventional Commit types help release and changelog tooling determine the impact of changes. This repository also provides a separate version-update task; do not assume a commit automatically updates the version or generates documentation.

## Working style

- Make small, precise changes that solve the stated problem.
- Do not silently change behavior that existing users may rely on.
- If a task is ambiguous, inspect the relevant command and surrounding patterns before making changes.
- Favor clarity over cleverness; keep logic straightforward and maintainable.