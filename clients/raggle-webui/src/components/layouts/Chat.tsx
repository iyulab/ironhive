import { useEffect, useState, KeyboardEvent, useRef } from 'react';
import { Button, Empty, Input, Space, Tooltip  } from "antd";
import { FileAddOutlined, BookOutlined, DatabaseOutlined, ClearOutlined, SendOutlined } from '@ant-design/icons';

import { ChatHistory } from '@/models/Model';
import { Hub } from '@/services/Hub';
import { API } from '@/services/API';
import { Message } from '../segments/Message';

import styles from './Chat.module.scss';
import { App } from '@/services/App';

const transformHistory = (history: any) => {
  return history.map((item: any) => {
    return {
      role: item.role.label,
      message: item.items[0].text,
    }
  });
}

export function Chat() {
  const historyRef = useRef<HTMLDivElement>(null);
  const [userMessage, setUserMessage] = useState<string>("");
  const [streamMessage, setStreamMessage] = useState<string>("");
  const [chatHistory, setChatHistory] = useState<ChatHistory[]>(transformHistory(App.assistant.chatHistory));
  const [chatDisabled, setChatDisabled] = useState<boolean>(false);

  const sendMessage = async () => {
    if (!userMessage || userMessage.trim().length === 0) return;
    setChatHistory((prev) => [...prev, { role: 'user', message: userMessage }]);
    const stream = Hub.chat(userMessage);
    let botMessage = "";
    setUserMessage("");
    setChatDisabled(true);
    stream?.subscribe({
      next: (response) => {
        botMessage += response;
        setStreamMessage(botMessage);
      },
      complete: () => {
        setChatHistory((prev) => [...prev, { role: 'bot', message: botMessage }]);
        setStreamMessage("");
        setChatDisabled(false);
      },
      error: (error) => {
        console.error(error);
        setStreamMessage("");
        setChatDisabled(false);
      },
    });
  }

  const onKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && e.shiftKey === false && chatDisabled === false) {
      sendMessage();
    }
  }

  const clearHistory = async () => {
    App.assistant.chatHistory = [];
    App.assistant = await API.updateAssistantAsync(App.assistant);
    setChatHistory([]);
  }

  useEffect(() => {
    historyRef.current?.scrollTo(0, historyRef.current.scrollHeight);
  }, [chatHistory, streamMessage]);

  useEffect(() => {
    Hub.connect();
    console.log(App.assistant);

    return () => {
      Hub.disconnect();
    }
  }, []);

  return (
    <div className={styles.container}>
      <div className={styles.history} ref={historyRef}>
        {chatHistory.length > 0 ? (
          <>
            {chatHistory.map((item, index) => (
              <Message 
                key={index}
                role={item.role}
                message={item.message}
              />
            ))}
            {streamMessage && (
              <Message
                role="bot"
                message={streamMessage} 
              />
            )}
          </>
        ) : (
          <Empty className={styles.empty}
            description={false}
          />
        )}
      </div>
      <div className={styles.input}>
        <Input.TextArea
          placeholder="Shift + Enter to add a new line"
          variant='borderless'
          autoSize={{ minRows: 3, maxRows: 10 }}
          value={userMessage}
          onChange={(e) => setUserMessage(e.target.value)}
          onKeyDown={onKeyDown}
        />
        <div className={styles.control}>
          <Space.Compact>
            <Tooltip title="add knowledge">
              <Button icon={<FileAddOutlined />} />
            </Tooltip>
            <Tooltip title="select knowledge">
              <Button icon={<BookOutlined />} />
            </Tooltip>
            <Tooltip title="select connection">
              <Button icon={<DatabaseOutlined />} />
            </Tooltip>
          </Space.Compact>
          <Space.Compact>
            <Tooltip title="Clear Chat">
              <Button icon={<ClearOutlined />} 
                onClick={clearHistory} />
            </Tooltip>
            <Tooltip title="Send Message">
              <Button icon={<SendOutlined />}
                loading={chatDisabled}
                onClick={sendMessage} />
            </Tooltip>
          </Space.Compact>
        </div>
      </div>
    </div>
  );
}