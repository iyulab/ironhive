import { CSSProperties } from 'react';
import { Button, Tabs } from 'antd';

import { Header } from './Header';
import { Chat } from './Chat';
import { ProfileForm } from '../forms/ProfileForm';
import { ConnectionList } from '../lists/ConnectionList';
import { KnowledgeList } from '../lists/KnowledgeList';
import { App } from '@/services/App';
import { API } from '@/services/API';

import styles from './Layout.module.scss';

export function Layout() {
  const tabPanelStyle: CSSProperties = {
    padding: "10px 50px",
    boxSizing: "border-box",
  }

  const updateAssistant = async () => {
    App.assistant = await API.updateAssistantAsync(App.assistant);
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <Header />
      </div>
      <div className={styles.main}>
        <div className={styles.chat}>
          <Chat />
        </div>
        <div className={styles.config}>
          <Tabs
            type="card"
            items={[
              { key: '1', label: 'Profile', 
                children: <ProfileForm assistant={App.assistant}/>, 
                style: tabPanelStyle},
              { key: '2', label: 'Knowledges', 
                children: <KnowledgeList />, 
                style: tabPanelStyle},
              { key: '3', label: 'Connections', 
                children: <ConnectionList />, 
                style: tabPanelStyle},
            ]}
          />
        </div>
        <div className={styles.control}>
          <Button type="primary" onClick={updateAssistant}>
            Update
          </Button>
        </div>
      </div>
    </div>
  );
}