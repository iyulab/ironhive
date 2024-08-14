import { useEffect, useState } from 'react';
import { Upload, Button, Form, Input, Space, notification } from 'antd';

import styles from './SourceForm.module.scss';
import { API } from '@/services/API';

export function SourceForm() {
  const [api, contextHolder] = notification.useNotification();
  const [mount, setMount] = useState(false);
  const [form] = Form.useForm();
  const [files, setFiles] = useState<any[]>([]);

  const renderFileItem = () => {
    console.log('render file item', files);
    return (
      <Form.Item 
        label="Files"
      >
        <Upload
          multiple={true}
          action={``}
          listType="picture"
          beforeUpload={checkFile}
          defaultFileList={files.map(f => ({
            uid: f.name,
            name: f.name,
            size: f.size,
            type: f.type,
            status: 'done' 
          }))}
          onChange={onFileChanged}
        >
          <Button>Upload</Button>
        </Upload>
      </Form.Item>
    )
  }

  const renderOpenApiSchema = () => {
    return (
      <Form.Item 
        label="Schema"
        name={['details', 'schema']}
        rules={[{ required: true }]}
      >
        <Input.TextArea />
      </Form.Item>
    );
  }

  const renderConnectionString = () => {
    return (
      <Form.Item 
        label="Connection String"
        name={['details', 'connectionString']}
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
    );
  }

  const checkFile = async (file: File) => {
    console.log('check file', file);
    const isSupported = await API.CheckFile(file.name);
    if (!isSupported) {
      alert('File not supported', 'Please upload a valid file type');
      return Upload.LIST_IGNORE;
    }
    const isExist = files.some(f => f.name === file.name);
    if (isExist) {
      alert('File already exists', 'Please upload a different file');
      return Upload.LIST_IGNORE;
    }
    return true;
  }

  const onFileChanged = async (info: any) => {
    console.log('file changed', info.file);
    if (info.file.status === 'done') {
      console.log('file uploaded', info.file);
      const file = {
        type: info.file.type,
        name: info.file.name,
        size: info.file.size,
      }
      setFiles([...files, file]);
    } else if (info.file.status === 'removed') {
      setFiles(files.filter(f => f.name !== info.file.name));
    }
  }

  const onFinish = (values: any) => {
    if (!source) return;
    source.name = values.name;
    source.description = values.description;
    if (source.type === 'file') {
      source.details = { files: files };
    } else if (source.type === 'openapi') {
      source.details = { schema: values.details.schema };
    } else if (source.type === 'sqlserver' || source.type === 'mongo') {
      source.details = { connectionString: values.details.connectionString };
    }
    onSubmit(source);
  }

  const alert = (title: string, message: string) => {
    api.open({
      message: title,
      description: message,
      showProgress: true,
      pauseOnHover: true,
    });
  }

  useEffect(() => {
    setMount(false);
    
  }, []);

  return mount && (
    <Form
      form={form}
      className={styles.form}
      layout="vertical"
      onFinish={onFinish}
    >
      <Form.Item
        layout="vertical"
        label="Name"
        name="name"
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item
        layout="vertical"
        label="Description"
        name='description'
      >
        <Input.TextArea />
      </Form.Item>
    </Form>
  )
}