import { useEffect, useState } from 'react';
import { Outlet } from 'react-router-dom';
import { 
  FluentProvider, 
  webLightTheme, webDarkTheme,
} from '@fluentui/react-components';
import { 
  PrimeReactProvider,
} from 'primereact/api';

import { Storage, Theme } from '@/services/Storage';
import { Header } from './Header';

import styles from './Layout.module.scss';

export function Layout() {
  const [theme, setTheme] = useState<Theme>(Storage.theme);
  const [fluentTheme, setFluentTheme] = useState(webLightTheme);
  const primeTheme = document.head.querySelector("#prime-theme");
  const githubTheme = document.head.querySelector("#github-theme");

  const handleChangeTheme = () => {
    const newTheme = theme === "light" ? "dark" : "light";
    setTheme(newTheme);
    if (newTheme === "light") {
      setFluentTheme(webLightTheme);
      primeTheme?.setAttribute("href", "styles/prime-light.css");
      githubTheme?.setAttribute("href", "styles/github-markdown-light.css");
    } else if (newTheme === "dark") {
      setFluentTheme(webDarkTheme);
      primeTheme?.setAttribute("href", "styles/prime-dark.css");
      githubTheme?.setAttribute("href", "styles/github-markdown-dark.css");
    }
  }

  useEffect(() => {
    window.addEventListener('change-theme', handleChangeTheme);

    return () => {
      window.removeEventListener('change-theme', handleChangeTheme);
    }
  });

  return (
    <FluentProvider theme={fluentTheme}>
      <PrimeReactProvider>
        <div className={styles.container}>
          <Header />
          <Outlet />
        </div>
      </PrimeReactProvider>
    </FluentProvider>
  );
}