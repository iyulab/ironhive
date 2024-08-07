import { useEffect, useRef, useState } from 'react';
import { Button, Textarea } from "@fluentui/react-components";
import styles from './Chat.module.scss';
import { Hub } from '@/services/Hub';
import Message from './Message';
import { API } from '@/services/API';
import { useNavigate } from 'react-router-dom';

interface ChatHistory {
  role: 'user' | 'bot';
  message: string;
}

export function Chat() {
  const navigate = useNavigate();
  const inputEl = useRef<HTMLTextAreaElement>(null);
  const [history, setHistory] = useState<ChatHistory[]>([]);
  const [stream, setStream] = useState<string>("");

  const sendMessage = async () => {
    const message = inputEl.current?.value;
    if (!message) return;
    setHistory((prev) => [...prev, { role: 'user', message }]);
    const stream = Hub.chat(message);
    let botMessage = "";
    stream?.subscribe({
      next: (response) => {
        botMessage += response;
        setStream(botMessage);
      },
      complete: () => {
        setStream("");
        setHistory((prev) => [...prev, { role: 'bot', message: botMessage }]);
      },
      error: (error) => {
        console.error(error);
      }
    });
  }

  const clearHistory = async () => {
    await API.clearHistory();
    getHistory();
  }

  const getHistory = async () => {
    const user = await API.getUser();
    if(user.chatHistory) {
      const history = user.chatHistory.map((chat: any) => {
        const role = chat.role.label;
        const message = chat.items[0].text;
        return {
          role: role,
          message: message,
        }
      })
      setHistory(history)
    }
  }

  useEffect(() => {
    getHistory();
  }, []);

  return (
    <div className={styles.container}>
      <div className={styles.control}>
        <Button onClick={() => navigate("source")}>Add Source</Button>
        <Button onClick={clearHistory}>Clear</Button>
      </div>
      <div className={styles.history}>
        {history.map((item, index) => {
          return (
            <Message key={index} 
              role={item.role} 
              message={item.message} />
          );
        })}
        {stream && (
          <Message role="bot" message={stream} />
        )}
      </div>
      <div className={styles.input}>
        <Textarea className={styles.textarea} ref={inputEl} />
        <Button onClick={sendMessage}>Send</Button>
      </div>
    </div>
  );
}