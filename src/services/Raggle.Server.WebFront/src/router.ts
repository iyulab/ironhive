import { createBrowserRouter } from 'react-router-dom';
import {
  Layout,
  Chat,
  Sources,
} from './components';

export const router = createBrowserRouter([
  {
    path: '/',
    Component: Layout,
    children: [
      {
        index: true,
        Component: Chat,
      },
      {
        path: "sources/:id?",
        Component: Sources
      }
    ]
  }
]);