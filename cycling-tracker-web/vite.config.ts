import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forwards /api/* to the ASP.NET Core backend so the browser only
      // ever talks to one origin — avoids CORS setup during dev.
      // Update the target if your API runs on a different port.
      "/api": {
        target: "http://localhost:5206",
        changeOrigin: true,
      },
    },
  },
});
