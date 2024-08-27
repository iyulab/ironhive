import { useEffect, useState } from "react";
import { Button, Form, Input, message, Space, Upload, UploadFile } from "antd";

import { Knowledge, KnowledgeFile } from "@/models/Model";
import { Storage } from "@/services/Storage";
import { API } from "@/services/API";

export function KnowledgeForm({ knowledge, onSubmit, onCancel }: { 
  knowledge: Knowledge, 
  onSubmit: (knowledge: Knowledge) => void, 
  onCancel: () => void
}) {
  const [form] = Form.useForm();
  const [files, setFiles] = useState<KnowledgeFile[]>(knowledge.files ?? []);
  const [uploadFiles, setUploadFiles] = useState<UploadFile[]>([]);

  const canUploadFile = async (file: File) => {
    if (files.some(f => f.name === file.name)) {
      message.error(`[${file.name}] file already exists`);
      return Upload.LIST_IGNORE;
    }
    try {
      await API.checkSupportFileAsync(file.name);
      return true;
    } catch (error: any) {
      message.error(`Failed to upload file: ${error?.response?.data}`);
      return Upload.LIST_IGNORE;
    }
  }

  const onFileChanged = (info: any) => {
    setUploadFiles(info.fileList);    
    setFiles(info.fileList
      .filter((file: UploadFile) => file.status === 'done')
      .map((file: UploadFile) => ({
        type: file.type,
        name: file.name,
        size: file.size,
      })));
  };

  const onFinish = async () => {
    const values = await form.validateFields();
    onSubmit({ ...knowledge, ...values, files });
  };

  useEffect(() => {
    form.resetFields();
    form.setFieldsValue(knowledge);
    setUploadFiles(knowledge.files?.map(f => ({
      uid: f.name,
      name: f.name,
      size: f.size,
      type: f.type,
      status: 'done'
    })) || []);
  }, [knowledge, form]);

  return (
    <>
      <Form
        form={form}
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
        <Form.Item 
          label="Files"
        >
          <Upload
            type="drag"
            listType="text"
            multiple={true}
            fileList={uploadFiles}
            action={`${Storage.host}/api/file/${knowledge.id}`}
            beforeUpload={canUploadFile}
            onChange={onFileChanged}
          >
            click or drag files to this area to upload
          </Upload>
        </Form.Item>
      </Form>
      <Space>
        <Button onClick={onCancel}>
          Cancel
        </Button>
        <Button type="primary" onClick={onFinish}>
          Submit
        </Button>
      </Space>
    </>
  );
}
