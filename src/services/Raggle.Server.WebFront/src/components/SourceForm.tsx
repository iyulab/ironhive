import { useEffect, useState } from 'react';
import styles from './SourceForm.module.scss';
import { Upload, Button, Form, Input, Space } from 'antd';
import { DataSource, FileSource, OpenApiSource, PostgresSource, SqlServerSource } from '@/models/DataSource';
import { Storage } from '@/services/Storage';
import { useParams } from 'react-router-dom';

export function SourceForm() {
  const { id } = useParams<{ id: string }>();
  const [source, setSource] = useState<DataSource | undefined>(undefined);

  useEffect(() => {
    if (id) {
      const source = Storage.getTempSource(id);
      setSource(source);
    }
  }, []);

  return (
    <div className={styles.container}>
      {source?.$type === 'file' && <FileSourceForm source={source} />}
      {source?.$type === 'openapi' && <OpenApiSourceForm source={source} />}
      {source?.$type === 'sqlserver' && <SqlServerSourceForm source={source} />}
      {source?.$type === 'postgres' && <PostgresSourceForm source={source} />} 
    </div>
  )
}

function FileSourceForm({ source }: { source: FileSource }) {

  useEffect(() => {

  }, [source]);

  return (
    <Form
      className={styles.form}
      name='file-source-form'
      layout="vertical" 
      initialValues={{ ...source }}
      onFinish={(values) => console.log('values', values)}
    >
      <Form.Item 
        label="Name"
        name='name'
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item 
        label="Files"
        valuePropName='fileList'
        rules={[{ required: true }]}
      >
        <Upload
          action={`${Storage.host}/file/${Storage.userId}/${source.id}`}
          listType="picture"
          beforeUpload={(f) => console.log('beforeUpload',f)}
          defaultFileList={[]}
        >
          <Button>Upload</Button>
        </Upload>
      </Form.Item>
      <Form.Item 
        label="Description"
        name='description'
        rules={[{ required: true }]}
        extra="Please describe the files you are uploading here. This is an important part that AI uses for data search and understanding. The more detailed the description, the better the AI can understand the data."
      >
        <Input.TextArea />
      </Form.Item>
      <Form.Item>
        <Space>
          <Button type="default" htmlType='submit'>
            Fill Description
          </Button>
          <Button type="primary" htmlType='submit'>
            Submit
          </Button>
        </Space>
      </Form.Item>
    </Form>
  )
}

function OpenApiSourceForm({ source }: { source: OpenApiSource }) {

  useEffect(() => {
    
  }, [source]);

  return (
    <Form
      className={styles.form}
      name='file-source-form'
      layout="vertical" 
      initialValues={{ name: source.name }}
      onFinish={(values) => console.log('values', values)}
    >
      <Form.Item 
        label="Name"
        name='name'
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item 
        label="Schema"
        name="schema"
        rules={[{ required: true }]}
      >
        <Input.TextArea />
      </Form.Item>
      <Form.Item 
        label="Description"
        name='description'
        rules={[{ required: true }]}
      >
        <Input.TextArea />
      </Form.Item>
      <Form.Item>
        <Button type="primary" htmlType='submit'>
          Submit
        </Button>
      </Form.Item>
    </Form>
  )
}

function SqlServerSourceForm({ source }: { source: SqlServerSource }) {

  useEffect(() => {

  }, [source]);

  return (
    <Form
      className={styles.form}
      name='file-source-form'
      layout="vertical" 
      initialValues={{ name: source.name }}
      onFinish={(values) => console.log('values', values)}
    >
      <Form.Item 
        label="Name"
        name='name'
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item 
        label="Connection String"
        name='connectionString'
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item 
        label="Description"
        name='description'
        rules={[{ required: true }]}
      >
        <Input.TextArea />
      </Form.Item>
      <Form.Item>
        <Button type="primary" htmlType='submit'>
          Submit
        </Button>
      </Form.Item>
    </Form>
  )
}

function PostgresSourceForm({ source }: { source: PostgresSource }) {

  useEffect(() => {

  }, [source]);

  return (
    <Form
      className={styles.form}
      name='file-source-form'
      layout="vertical" 
      initialValues={{ name: source.name }}
      onFinish={(values) => console.log('values', values)}
    >
      <Form.Item 
        label="Name"
        name='name'
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item 
        label="Connection String"
        name='connectionString'
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item 
        label="Description"
        name='description'
        rules={[{ required: true }]}
      >
        <Input.TextArea />
      </Form.Item>
      <Form.Item>
        <Button type="primary" htmlType='submit'>
          Submit
        </Button>
      </Form.Item>
    </Form>
  )
}