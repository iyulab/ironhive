import { createBrowserRouter } from 'react-router-dom';
import {
  Layout,
  Chat,
  SourceList,
  SourceForm,
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
        path: "source",
        Component: SourceList,
      },
      {
        path: "source/:id",
        Component: SourceForm,
      }
    ]
  }
]);