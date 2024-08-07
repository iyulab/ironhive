import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc'
import { resolve } from 'path'

export default defineConfig({
  root: 'src',
  publicDir: resolve(__dirname, 'public'),
  envDir: resolve(__dirname),
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
    },
  },
  build: {
    target: 'esnext',
    outDir: resolve(__dirname, 'publish'),
  },
  plugins: [
    react()
  ],
});