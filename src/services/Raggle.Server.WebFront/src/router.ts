import { createBrowserRouter } from 'react-router-dom';
import { 
  Layout 
} from './components/layouts/Layout';

export const router = createBrowserRouter([
  {
    path: '/',
    Component: Layout
  }
]);