import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  root: resolve(__dirname, 'src'),
  publicDir: resolve(__dirname, 'public'),
  envDir: resolve(__dirname),
  build: {
    copyPublicDir: true,
    emptyOutDir: true,
    outDir: '../dist',
    rollupOptions: {
      input: resolve(__dirname, 'src/index.html'),
    }
  },
  plugins: []
});
