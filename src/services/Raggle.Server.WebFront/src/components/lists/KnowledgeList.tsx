import { useEffect, useState } from "react";
import { Button, Empty, List, Modal } from "antd";
import { BookOutlined } from "@ant-design/icons";
import VirtualList from 'rc-virtual-list';

import { Knowledge } from "@/models/Model";
import { API } from "@/services/API";
import { KnowledgeForm } from "../forms/KnowledgeForm";

export function KnowledgeList() {
  const [openModal, setOpenModal] = useState(false);
  const [knowledges, setKnowledges] = useState<Knowledge[]>([]);
  const [knowledge, setKnowledge] = useState<Knowledge | null>(null);
  
  const ContainerHeight = 500;

  const appendData = () => {

  }

  const onScroll = (e: React.UIEvent<HTMLElement, UIEvent>) => {
    if (Math.abs(e.currentTarget.scrollHeight - e.currentTarget.scrollTop - ContainerHeight) <= 1) {
      appendData();
    }
  };

  const updateKnowledge = async (knowledge: Knowledge) => {
    setKnowledge(knowledge);
    setOpenModal(true);
  }

  const createNewKnowledge = async () => {
    setKnowledge(null);
    setOpenModal(true);
  }

  const fetchKnowledges = async () => {
    const knowledges = await API.getKnowledgesAsync(0, 10);
    setKnowledges(knowledges);
  }

  useEffect(() => {
    fetchKnowledges();
  }, []);
  
  return knowledges.length > 0 ? (
    <>
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
              <Button type="primary" onClick={() => console.log("click button")}>
                Connect
              </Button>
            </List.Item>
          )}
        </VirtualList>
      </List>
      <Modal
        title={knowledge ? 'Update Knowledge' : 'Create Knowledge'}
        open={openModal}
        onCancel={() => setOpenModal(false)}
        footer={null}
      >
        { knowledge && <KnowledgeForm knowledge={knowledge} /> }
      </Modal>
    </>
  ) : (
    <Empty
      image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg"
      imageStyle={{ height: 100 }}
      description="No knowledges found, create one!"
    >
      <Button type="primary" onClick={createNewKnowledge}>
        Create New
      </Button>
    </Empty>
  );
}