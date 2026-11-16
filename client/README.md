# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some Oxlint rules.

## Themes

Use **Theme settings** in the top-right corner to choose System, Light, Dark,
Exotic, or Nature. The selection applies throughout the app and is saved in
browser storage. System follows the device's light or dark preference.

## Products

Start the API with its HTTPS profile on `https://localhost:5001`, then run
`npm run dev` in `client` and open `http://localhost:3000`. The Products menu
opens a searchable, paginated catalog; select a product for its details. Prices
are displayed in USD from the API's cent values.

Vite proxies `/api` and `/images` to the local API in development, accepting
its development certificate. Product images show a placeholder if unavailable.
For production, configure the web server to forward these paths to the API and
serve the SPA entry point for client-side routes, including `/products/:id`.
Run the product API service tests with `npm test` on Node.js 22.6 or newer.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is enabled on this template. See [this documentation](https://react.dev/learn/react-compiler) for more information.

Note: This will impact Vite dev & build performances.
You can also try [the experimental native React Compiler support in plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react/README.md#rust-react-compiler) by using `compiler: true` in the plugin options instead of using the Babel plugin.

## Expanding the Oxlint configuration

If you are developing a production application, we recommend enabling type-aware lint rules by installing `oxlint-tsgolint` and editing `.oxlintrc.json`:

```json
{
  "$schema": "./node_modules/oxlint/configuration_schema.json",
  "plugins": ["react", "typescript", "oxc"],
  "options": {
    "typeAware": true
  },
  "rules": {
    "react/rules-of-hooks": "error",
    "react/only-export-components": ["warn", { "allowConstantExport": true }]
  }
}
```

See the [Oxlint rules documentation](https://oxc.rs/docs/guide/usage/linter/rules) for the full list of rules and categories.
