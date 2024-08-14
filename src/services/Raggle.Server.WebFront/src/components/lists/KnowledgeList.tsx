import { useEffect, useState } from "react";
import { Button, Empty, List, Modal, Typography } from "antd";
import { BookOutlined } from "@ant-design/icons";
import VirtualList from 'rc-virtual-list';

import { Knowledge } from "@/models/Model";
import { API } from "@/services/API";

export function KnowledgeList() {
  const [knowledges, setKnowledges] = useState<Knowledge[]>([]);
  
  const ContainerHeight = 500;

  const appendData = () => {

  }

  const onScroll = (e: React.UIEvent<HTMLElement, UIEvent>) => {
    if (Math.abs(e.currentTarget.scrollHeight - e.currentTarget.scrollTop - ContainerHeight) <= 1) {
      appendData();
    }
  };

  const createNewKnowledge = async () => {
    Modal.info({
      title: "Create New Knowledge",
      content: (
        <Typography.Paragraph>
          This feature is not implemented yet.
        </Typography.Paragraph>
      ),
      onOk() {
        console.log("OK");
      },
    })
  }

  const fetchKnowledges = async () => {
    const knowledges = await API.getKnowledgesAsync(0, 10);
    setKnowledges(knowledges);
  }

  useEffect(() => {
    fetchKnowledges();
  }, []);
  
  return knowledges.length > 0 ? (
    <List>
      <VirtualList
        data={knowledges}
        height={ContainerHeight}
        itemHeight={47}
        itemKey="id"
        onScroll={onScroll}
      >
        {(item) => (
          <List.Item key={item.id} onClick={() => console.log("click list")}>
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