# Code Standard

These rules define the coding standards for this project. All code must follow these conventions to ensure consistency, maintainability, and clarity across the codebase.

## Classes

- Classes must be `sealed` by default unless they are explicitly designed for inheritance.
- Only omit `sealed` when a class is a base class intended to be extended.

## Documentation

- All public classes and methods must have a meaningful XML `<summary>` doc comment.
- Summaries must describe intent or behavior, not just restate the name.
