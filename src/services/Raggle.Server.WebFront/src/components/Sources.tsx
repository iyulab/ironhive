import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Card, List, Modal } from 'antd';

import { API } from '@/services/API';
import { Storage } from '@/services/Storage';
import { DataSource, sourcesList, SourceType } from '@/models/DataSource';

import styles from './Sources.module.scss';
import { SourceForm } from './SourceForm';

export function Sources() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [modalOpen, setModalOpen] = useState(false);
  const [sources, setSources] = useState<DataSource[]>([]);
  const [source, setSource] = useState<DataSource>();

  const createNew = (type: SourceType) => {
    const id = Storage.getRandomUUID();
    Storage.setTempSource({
      id: id,
      userID: Storage.userId,
      type: type,
      details: {}
    });
    setModalOpen(false);
    navigate(`/sources/${id}`, { replace: true });
  }

  const onSubmit = async (source: DataSource) => {
    if (source?.createdAt) {
      console.log('update source');
      await API.updateSource(source);
    } else {
      console.log('create source');
      await API.createSource(source);
    }
    refresh(source.id);
  }

  const onDelete = async () => {
    if (!source) return;
    console.log('DELETE source', source);
    await API.deleteSource(source.id);
    navigate('/sources', { replace: true });
  }

  const refresh = async (id?: string) => {
    if (id) {
      let _source: DataSource | undefined;
      try {
        _source = await API.getSource(id);
        console.log('GET source', _source);
      } catch {
        _source = Storage.getTempSource(id);
        console.log('GET Temp source', _source);
      }
      setSource(_source);
    } else {
      setSource(undefined);
    }
    const _sources = await API.getSources(Storage.userId);
    setSources(_sources);
  }

  useEffect(() => {
    refresh(id);
  }, [id]);

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <Button onClick={() => navigate('/')}>&lt; Back</Button>
        <div className={styles.title}>
          Data Sources
        </div>
        <div className={styles.control}>
          <Button onClick={() => setModalOpen(true)}>Create</Button>
          <Modal
            title="Select Source Type"
            open={modalOpen}
            onCancel={() => setModalOpen(false)}
            footer={null}
          >
            <Card>
              {sourcesList.map((source, index) => (
                <Card.Grid key={index}
                  className={styles.card}
                  onClick={() => createNew(source.type)}>
                  {source.img}
                  {source.label}
                </Card.Grid>
              ))}
            </Card>
          </Modal>
        </div>
      </div>
      <div className={styles.list}>
        <List
          rowKey={(item) => item.id}
          pagination={{ position: "top", align: "end" }}
          dataSource={sources}
          renderItem={(item) => (
            <List.Item
              className={styles.item}
              onClick={() => navigate(`/sources/${item.id}`)}
            >
              <List.Item.Meta
                title={item.name}
                description={item.description}
              />
            </List.Item>
          )}
        />
      </div>
      <div className={styles.form}>
        <SourceForm 
          source={source} 
          onSubmit={onSubmit} 
          onDelete={onDelete}
        />
      </div>
    </div>
  )
}