import {
  Card,
  CardFooter,
  CardPreview,
} from "@fluentui/react-components";

import styles from './SourceList.module.scss';
import { Storage } from "@/services/Storage";
import { SourceType } from "@/models/DataSource";
import { useNavigate } from "react-router-dom";

type SourceDisplay = {
  label: string;
  img: string;
  type: SourceType;
}

const sourcesList: SourceDisplay[] = [
  { label: "File", img: "", type: "file" },
  { label: "Open API", img: "", type: "openapi" },
  { label: "Microsoft SQL Server", img: "", type: "sqlserver" },
  { label: "PostgreSQL", img: "", type: "postgres" },
]

export function SourceList() {
  const navigate = useNavigate();

  const handleCardClick = (type: SourceType) => {
    const id = Storage.getRandomUUID();
    Storage.setTempSource({ id, $type: type });
    navigate(`/source/${id}`);
  }

  return (
    <div className={styles.container}>
      {sourcesList.map((source, index) => (
        <Card key={index} 
          className={styles.card}
          onClick={() => handleCardClick(source.type)}>
          <CardPreview>
            {source.img}
          </CardPreview>
          <CardFooter>
            {source.label}
          </CardFooter>
        </Card>
      ))}
    </div>
  )
}