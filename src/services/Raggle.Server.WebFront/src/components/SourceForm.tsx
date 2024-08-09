import { useEffect, useState } from 'react';
import { Upload, Button, Form, Input, Space } from 'antd';

import { DataSource, FileMeta } from '@/models/DataSource';
import { Storage } from '@/services/Storage';

import styles from './SourceForm.module.scss';

export function SourceForm({ source, onSubmit, onDelete } : { 
  source?: DataSource,
  onSubmit: (source: DataSource) => void,
  onDelete: () => void
}) {
  const [mount, setMount] = useState(false);
  const [form] = Form.useForm();
  const [files, setFiles] = useState<FileMeta[]>([]);

  const renderFileItem = () => {
    console.log('render file item', files);
    return (
      <Form.Item 
        label="Files"
      >
        <Upload
          multiple={true}
          action={`${Storage.host}/file/${source?.id}`}
          listType="picture"
          beforeUpload={checkDuplicateFiles}
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

  const checkDuplicateFiles = (file: File) => {
    const isExist = files.some(f => f.name === file.name);
    console.log('check duplicate', file, isExist);
    return isExist ? Upload.LIST_IGNORE : true;
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

  useEffect(() => {
    setMount(false);
    if (source) {
      console.log('mount source form', source);
      form.resetFields();
      console.log('source details', source.details.files);
      setFiles(source?.details?.files || []);
      setMount(true);
    } else {
      console.log('unmount source form');
      setFiles([]);
    }
  }, [source, form]);

  return mount && (
    <Form
      form={form}
      className={styles.form}
      layout="vertical"
      initialValues={{ ...source }}
      onFinish={onFinish}
    >
      <Form.Item>
        <Space direction='horizontal' align='end'>
          <Button type="default">
            Fill Description
          </Button>
          <Button type="default" onClick={onDelete}>
            Delete
          </Button>
          <Button type="primary" htmlType='submit'>
            Submit
          </Button>
        </Space>
      </Form.Item>
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
      {source?.type === 'file'
        ? renderFileItem()
        : source?.type === 'openapi'
        ? renderOpenApiSchema()
        : source?.type === 'sqlserver' || source?.type === 'mongo'
        ? renderConnectionString()
        : null
      }
    </Form>
  )
}