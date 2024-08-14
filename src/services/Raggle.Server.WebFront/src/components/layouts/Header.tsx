import { useNavigate } from 'react-router-dom';
import { Button, Tooltip } from 'antd';
import { GithubFilled } from '@ant-design/icons';

import styles from './Header.module.scss';

export function Header() {
  const navigate = useNavigate();

  return (
    <div className={styles.container}>
      <div className={styles.title}
        onClick={() => navigate('/')}>
        <img src="/icons/logo.svg" />
        <p>RAGGLE</p>
      </div>
      <div className={styles.control}>
        <Tooltip title="GitHub">
          <Button 
            type="default" 
            shape="circle"
            target="_blank"
            href='https://github.com/iyulab-rnd/Raggle'
            icon={<GithubFilled />} />
        </Tooltip>
      </div>
    </div>
  )
}