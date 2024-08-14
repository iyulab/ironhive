import { useEffect, useState } from "react";
import { Button, Empty, List } from "antd";
import { BookOutlined } from "@ant-design/icons";
import VirtualList from 'rc-virtual-list';

import { Connection } from "@/models/Model";
import { API } from "@/services/API";

export function ConnectionList() {
  const [connections, setConnections] = useState<Connection[]>([]);

  const ContainerHeight = 500;
  
  const appendData = () => {

  }

  const onScroll = (e: React.UIEvent<HTMLElement, UIEvent>) => {
    if (Math.abs(e.currentTarget.scrollHeight - e.currentTarget.scrollTop - ContainerHeight) <= 1) {
      appendData();
    }
  };

  const createNewKnowledge = () => {
    console.log("Create New Knowledge");
  }

  const fetchConnections = async () => {
    const connections = await API.getConnectionsAsync(0, 10);
    setConnections(connections);
  }

  useEffect(() => {
    fetchConnections();
  }, []);

  return connections.length > 0 ? (
    <List>
      <VirtualList
        data={connections}
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
      description="No connections found, create one!"
    >
      <Button type="primary" onClick={createNewKnowledge}>
        Create New
      </Button>
    </Empty>
  );
}