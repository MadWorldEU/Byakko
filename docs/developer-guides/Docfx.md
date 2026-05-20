# DocFX

This document provides information about the DocFX tool used for generating documentation in this project.

## Install

```bash
dotnet tool update -g docfx
```

## Run locally

From the `docs/` folder:

```bash
docfx docfx.json --serve
```

This builds the documentation site and serves it at `http://localhost:8080`.

## Live documentation

The published documentation is available at [https://madworldeu.github.io/byakko](https://madworldeu.github.io/byakko).