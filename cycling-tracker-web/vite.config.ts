/// <reference types="vitest" />
// 
import { defineConfig, PluginOption } from "vite";
import react from "@vitejs/plugin-react";
import basicSsl from '@vitejs/plugin-basic-ssl';

export default defineConfig(() => {
  return {
  plugins: [react()],//, basicSsl() as PluginOption],
  server: {
    port: 5173,
    host: "127.0.0.1", // Forces Vite to use local IP instead of guessing 'localhost'
    hmr: {
      host: "127.0.0.1", // Forces the hot-reload socket to use the exact same IP
      port: 5173
    },
    //https: true
  },
    test: {
    // 1. Allows you to use 'describe', 'it', and 'expect' without importing them in every test file
    globals: true, 
    
    // 2. Simulates a browser environment in Node (essential when you start testing React components)
    // environment: 'jsdom', 
    
    // 3. Tells Vitest where to find your setup file (optional, but highly recommended for custom matchers)
    // setupFiles: './src/setupTests.js', 
  },
}});






