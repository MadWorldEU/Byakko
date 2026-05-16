# Structurizr

The C4 model diagrams are defined using the [Structurizr DSL](https://github.com/structurizr/dsl) and stored in `docs/diagrams/`. Structurizr Lite is used to render and edit them locally via Docker.

## Running locally

Run the following command from the `docs/diagrams/` folder:

```bash
docker run -it --rm -p 8080:8080 -v ./:/usr/local/structurizr structurizr/structurizr local
```

Then open [http://localhost:8080](http://localhost:8080) in your browser.

## Editing diagrams

There are two ways to edit the diagrams:

- **DSL** — edit `workspace.dsl` directly. The browser auto-reloads on save.
- **Browser** — drag elements around on the canvas to adjust the layout. Changes are saved back to `workspace.json`.

## Exporting

To export a diagram as SVG or PNG:

1. Open the diagram in the browser.
2. Click the export button (top-right toolbar).
3. Choose **SVG** for a scalable vector export, or **PNG** for a raster image.
4. Save the exported file to `docs/diagrams/images/`.

## DSL examples

DSL examples can be found at [https://docs.structurizr.com/dsl/example](https://docs.structurizr.com/dsl/example).