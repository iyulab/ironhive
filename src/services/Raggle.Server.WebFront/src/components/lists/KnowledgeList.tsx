import { useEffect, useState } from "react";
import { Button, Empty, List, Modal } from "antd";
import { BookOutlined } from "@ant-design/icons";
import VirtualList from 'rc-virtual-list';

import { Knowledge } from "@/models/Model";
import { API } from "@/services/API";
import { KnowledgeForm } from "../forms/KnowledgeForm";
import { Storage } from "@/services/Storage";

const createNewKnowledge = () => {
  return { id: Storage.getRandomUUID(), name: 'unknown', files: [] };
}

export function KnowledgeList() {
  const [openModal, setOpenModal] = useState(false);
  const [knowledges, setKnowledges] = useState<Knowledge[]>([]);
  const [knowledge, setKnowledge] = useState<Knowledge | undefined>(createNewKnowledge());
  
  const ContainerHeight = 500;

  const appendData = () => {

  }

  const onScroll = (e: React.UIEvent<HTMLElement, UIEvent>) => {
    if (Math.abs(e.currentTarget.scrollHeight - e.currentTarget.scrollTop - ContainerHeight) <= 1) {
      appendData();
    }
  };

  const updateKnowledge = async (knowledge?: Knowledge) => {
    setKnowledge(knowledge || createNewKnowledge());
    setOpenModal(true);
  }

  const onSubmit = async (knowledge: Knowledge) => {
    if (knowledge.createdAt) {
      await API.updateKnowledgeAsync(knowledge);
    } else {
      await API.createKnowledgeAsync(knowledge);
    }
    fetchKnowledges();
    setKnowledge(undefined);
    setOpenModal(false);
  }

  const onCancel = () => {
    console.log(knowledge);
    setKnowledge(undefined);
    setOpenModal(false);
  }

  const fetchKnowledges = async () => {
    const knowledges = await API.getKnowledgesAsync(0, 10);
    setKnowledges(knowledges);
  } 

  useEffect(() => {
    fetchKnowledges();
  }, []);
  
  return (
    <>
      <Modal
        title={knowledge ? 'Update Knowledge' : 'Create Knowledge'}
        open={openModal}
        onCancel={onCancel}
        footer={null}
      >
        {<KnowledgeForm 
          knowledge={knowledge} 
          onSubmit={updateKnowledge} 
          onCancel={onCancel} 
        />}
      </Modal>
  
      {knowledges.length > 0 ? (
        <List>
          <VirtualList
            data={knowledges}
            height={ContainerHeight}
            itemHeight={47}
            itemKey="id"
            onScroll={onScroll}
          >
            {(item) => (
              <List.Item key={item.id} onClick={() => updateKnowledge(item)}>
                <List.Item.Meta
                  avatar={<BookOutlined />}
                  title={item.name}
                  description={item.description}
                />
                <Button type="primary" onClick={() => console.log('click button')}>
                  Connect
                </Button>
              </List.Item>
            )}
          </VirtualList>
        </List>
      ) : (
        <Empty
          image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg"
          imageStyle={{ height: 100 }}
          description="No knowledges found, create one!"
        >
          <Button type="primary" onClick={() => updateKnowledge()}>
            Create New
          </Button>
        </Empty>
      )}
    </>
  );  
}