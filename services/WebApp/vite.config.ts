import { defineConfig } from 'vite';
import dts from "vite-plugin-dts";
import { resolve } from 'path';

export default defineConfig({
  root: resolve(__dirname, 'src'),
  envDir: resolve(__dirname),
  publicDir: resolve(__dirname, 'public'),
  build: {
    copyPublicDir: true,
    emptyOutDir: true,
    outDir: '../dist',
    rollupOptions: {
      input: resolve(__dirname, 'src/index.html'),
    }
  },
  plugins: [
    dts({
      include: [ "src/**/*"] 
    }),
  ]
});
