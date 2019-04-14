## Contributing to xNode
💙Thank you for taking the time to contribute💙

If you haven't already, join our [Discord channel](https://discord.gg/qgPrHv4)!

## Pull Requests
Try to keep your pull requests relevant, neat, and manageable. If you are adding multiple features, try splitting them into separate commits.
* Avoid including irellevant whitespace or formatting changes.
* Comment your code.
* Spell check your code / comments

## New features
xNode aims to be simple and extendible, not trying to fix all of Unity's shortcomings.

If your feature aims to cover something not related to editing nodes, it generally won't be accepted. If in doubt, ask on the Discord channel.

## Coding conventions
Skim through the code and you'll get the hang of it quickly.
* Methods, Types and properties PascalCase
* Variables camelCase
* Public methods XML commented. Params described if not obvious
* Explicit usage of brackets when doing multiple math operations on the same line

## Formatting
I use VSCode with the C# FixFormat extension and the following setting overrides:
```json
"csharpfixformat.style.spaces.beforeParenthesis": false,
"csharpfixformat.style.indent.regionIgnored": true
```
* Open braces on same line as condition
* 4 spaces for indentation.
