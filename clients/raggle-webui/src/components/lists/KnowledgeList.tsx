import { useEffect, useState } from "react";
import { Button, Empty, Input, List, Modal, Space } from "antd";
import { BookOutlined } from "@ant-design/icons";
import VirtualList from 'rc-virtual-list';

import { Knowledge } from "@/models/Model";
import { API } from "@/services/API";
import { KnowledgeForm } from "../forms/KnowledgeForm";
import { App } from "@/services/App";

export function KnowledgeList() {
  const [openModal, setOpenModal] = useState(false);
  const [knowledges, setKnowledges] = useState<Knowledge[]>([]);
  const [knowledge, setKnowledge] = useState<Knowledge | undefined>();
  
  const ContainerHeight = 500;
  const onScroll = (e: React.UIEvent<HTMLElement, UIEvent>) => {
    const target = e.currentTarget;
    if (Math.abs(target.scrollHeight - target.scrollTop - ContainerHeight) <= 1) {
      console.log('scroll to bottom');
    }
  };

  const fetchKnowledges = async () => {
    const knowledges = await API.getKnowledgesAsync(0, 10);
    setKnowledges(knowledges);
  }

  const editKnowledge = async (knowledge: Knowledge) => {
    setKnowledge(knowledge);
    setOpenModal(true);
  }

  const createKnowledge = async () => {
    const newKnowledge = await API.createKnowledgeAsync();
    App.assistant.knowledges.push(newKnowledge.id);
    App.assistant = await API.updateAssistantAsync(App.assistant);
    setKnowledge(newKnowledge);
    setOpenModal(true);
  }

  const updateKnowledge = async (knowledge: Knowledge) => {
    await API.updateKnowledgeAsync(knowledge);
    setOpenModal(false);
    setKnowledge(undefined);
    fetchKnowledges();
  }

  const deleteKnowledge = async (knowledgeId: string) => {
    await API.deleteKnowledgeAsync(knowledgeId);
    App.assistant.knowledges = App.assistant.knowledges.filter(id => id !== knowledgeId);
    App.assistant = await API.updateAssistantAsync(App.assistant);
    fetchKnowledges();
  }

  const cancelKnowledge = async () => {
    if (knowledge && !knowledge.updatedAt) {
      await API.deleteKnowledgeAsync(knowledge.id);
    }
    setOpenModal(false);
    setKnowledge(undefined);
  }

  useEffect(() => {
    fetchKnowledges();
  }, []);
  
  return (
    <>
      {knowledge && (
        <Modal
          title={knowledge?.createdAt ? 'Update Knowledge' : 'Create Knowledge'}
          open={openModal}
          closable={false}
          footer={null}
        >
          <KnowledgeForm
            knowledge={knowledge}
            onSubmit={updateKnowledge}
            onCancel={cancelKnowledge}
          />
        </Modal>
      )}
  
      {knowledges.length > 0 ? (
        <>
          <Space>
            <Input.Search placeholder="Search knowledge" />
            <Button type="primary" onClick={createKnowledge}>
              Create New
            </Button>
          </Space>
          <List>
            <VirtualList
              data={knowledges}
              height={ContainerHeight}
              itemHeight={47}
              itemKey="id"
              onScroll={onScroll}
            >
              {(item) => (
                <List.Item key={item.id}>
                  <List.Item.Meta
                    avatar={<BookOutlined />}
                    title={item.name}
                    description={item.description}
                  />
                  <Button type="link" onClick={() => editKnowledge(item)}>
                    Edit
                  </Button>
                  <Button type="link" onClick={() => deleteKnowledge(item.id)}>
                    Delete
                  </Button>
                </List.Item>
              )}
            </VirtualList>
          </List>
        </>
      ) : (
        <Empty
          image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg"
          imageStyle={{ height: 100 }}
          description="No knowledges found, create one!"
        >
          <Button type="primary" onClick={createKnowledge}>
            Create New
          </Button>
        </Empty>
      )}
    </>
  );  
}