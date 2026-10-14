/// <reference types="vitest" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // Forward API calls to the .NET API so the browser only ever sees one origin (no CORS).
    proxy: {
      "/api": {
        target: "http://localhost:5206",
        changeOrigin: false, // keep the original Host header
      },
    },
  },
  test: {
    globals: true,
  },
});