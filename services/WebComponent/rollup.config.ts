import { globSync } from 'fs';
import { fileURLToPath } from 'url';
import path from 'path';
import { RollupOptions } from 'rollup';
// import dts from 'rollup-plugin-dts';
// import commonjs from '@rollup/plugin-commonjs';
// import { nodeResolve } from '@rollup/plugin-node-resolve';
import typescript from 'rollup-plugin-typescript2';

const files = Object.fromEntries(
  globSync('src/**/*.ts').map(file => [
    path.relative(
      'src',
      file.slice(0, file.length - path.extname(file).length)
    ),
    fileURLToPath(new URL(file, import.meta.url))
  ])
);

console.log('Files: ', files);

const config : RollupOptions = {
  input: "src/index.ts",
  output: [
    {
      format: 'es',
      dir: 'dist',
    }
  ],
  // external: [
  //   /^lit.*/,
  //   /^@lit.*/,
  //   'react',
  //   'mobx',
  //   'reflect-metadata',
  // ],
  plugins: [
    typescript({
      tsconfig: "tsconfig.json",
    }),
    // nodeResolve(),
    // commonjs(),
  ],
  treeshake: true,
};

export default config;
