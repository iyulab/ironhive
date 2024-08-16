import { useState } from "react";
import { Button, Form, Input, Upload } from "antd";

import { Knowledge } from "@/models/Model";

export function KnowledgeForm({ knowledge }: { knowledge?: Knowledge }) {
  const [form] = Form.useForm();
  const [files, setFiles] = useState<any[]>([]);
  
  const checkFile = async (file: File) => {
    console.log('check file', file);
    // const isSupported = await API.CheckFile(file.name);
    if (!file.type.startsWith('image/')) {
      // alert('File not supported', 'Please upload a valid file type');
      return Upload.LIST_IGNORE;
    }
    const isExist = files.some(f => f.name === file.name);
    if (isExist) {
      // alert('File already exists', 'Please upload a different file');
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
    console.log('on finish', values);
  };

  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={onFinish}
      initialValues={knowledge}
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
    </Form>
  );
}