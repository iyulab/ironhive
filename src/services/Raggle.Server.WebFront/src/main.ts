import React from 'react';
import { createRoot } from 'react-dom/client';
import { Hub } from './services/Hub';
import { RouterProvider } from 'react-router-dom';
import { router } from './router';

const root = document.getElementById('root');

if (root) {
  const rootDOM = createRoot(root);
  const routeDOM = React.createElement(RouterProvider, { router: router })
  const app = import.meta.env.DEV
    ? React.createElement(React.StrictMode, null, routeDOM)
    : routeDOM;

  Hub.connect();
  rootDOM.render(app);
} else {
  document.body.innerHTML = 'No root element found';
}