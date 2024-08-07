import { Button } from '@fluentui/react-button';
import {
  CalendarMonthFilled,
  CalendarMonthRegular,
} from "@fluentui/react-icons";

import styles from './Header.module.scss';
import { useNavigate } from 'react-router-dom';

export function Header() {
  const navigate = useNavigate();

  const changeTheme = () => {
    window.dispatchEvent(new Event('change-theme'));
  }

  return (
    <div className={styles.container}>
      <div className={styles.title}
        onClick={() => navigate('/')}>
        <img src="/icons/logo.svg" />
        <p>RAGGLE</p>
      </div>
      <div className={styles.control}>
        <Button as="a"
          href='https://github.com/iyulab-rnd/Raggle' 
          icon={<CalendarMonthRegular />}
          appearance='subtle'>
          GitHub
        </Button>
        <Button onClick={changeTheme}
          icon={<CalendarMonthFilled />}
          appearance='subtle'>
          Theme
        </Button>
      </div>
    </div>
  )
}