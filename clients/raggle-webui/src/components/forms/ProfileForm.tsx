import { Assistant } from "@/models/Model";
import { App } from "@/services/App";
import { Form, Input } from "antd";
import { useEffect } from "react";

export function ProfileForm({ assistant } : { assistant: Assistant }) {
  const [form] = Form.useForm();

  const onChange = (values: any) => {
    Object.assign(App.assistant, values);
  }

  useEffect(() => {
    
  }, [assistant]);

  return (
    <Form
      form={form}
      layout="vertical"
      onValuesChange={onChange}
      initialValues={assistant}
    >
      <Form.Item
        layout="vertical"
        label="Name"
        name="name"
      >
        <Input />
      </Form.Item>
      <Form.Item
        layout="vertical"
        label="Instruction"
        name="instruction"
      >
        <Input.TextArea 
          autoSize={{ minRows: 5 }}
        />
      </Form.Item>
    </Form>
  )
}