import React from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider } from 'react-router-dom';

import { router } from './router';
import { App } from './services/App';

const root = document.getElementById('root');

if (root) {
  const rootDOM = createRoot(root);
  const routeDOM = React.createElement(RouterProvider, { router: router })
  const app = import.meta.env.DEV
    ? React.createElement(React.StrictMode, null, routeDOM)
    : routeDOM;

  try {
    await App.initAsync();
    rootDOM.render(app);
  } catch (error: any) {
    console.error(error);
    root.innerHTML = 'Failed to initialize the app';
  }
} else {
  document.body.innerHTML = 'No root element found';
}