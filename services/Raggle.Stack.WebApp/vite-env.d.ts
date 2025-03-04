/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly BASE_URL: string;
  readonly VITE_API_HOST: string;
  readonly VITE_API_VERSION: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
