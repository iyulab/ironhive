import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

import styles from './Message.module.scss';

export default function Message({ role, message }: { role: string, message: string }) {
  return (
    <div className={styles.container}>
      <div className={styles.role}>
        {role}
      </div>
      <div className={`markdown-body ${styles.message}`}>
        <ReactMarkdown remarkPlugins={[remarkGfm]}>
          {message}
        </ReactMarkdown>
      </div>
    </div>
  );
}