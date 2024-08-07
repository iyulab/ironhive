import styles from './SourceMenu.module.scss';
import {
  DataGridBody,
  DataGridRow,
  DataGrid,
  DataGridHeader,
  DataGridHeaderCell,
  DataGridCell,
  TableColumnDefinition,
  createTableColumn,
  Button,
} from "@fluentui/react-components";

const items = [
  {
    id: window.crypto.getRandomValues(new Uint32Array(1))[0],
    name: "Document",
    type: "Word",
    lastUpdate: new Date("2021-09-01T12:00:00Z"),
  },
  {
    id: window.crypto.getRandomValues(new Uint32Array(1))[0],
    name: "Spreadsheet",
    type: "Excel",
    lastUpdate: new Date("2021-09-02T12:00:00Z"),
  },
  {
    id: window.crypto.getRandomValues(new Uint32Array(1))[0],
    name: "Presentation",
    type: "PowerPoint",
    lastUpdate: new Date("2021-09-03T12:00:00Z"),
  },
];

const columns: TableColumnDefinition<any>[] = [
  createTableColumn({
    columnId: "type",
    compare: (a, b) => {
      return a.type.localeCompare(b.type);
    },
    renderHeaderCell: () => {
      return "Type";
    },
    renderCell: (item) => {
      return item.type;
    },
  }),
  createTableColumn({
    columnId: "name",
    compare: (a, b) => {
      return a.name.localeCompare(b.name);
    },
    renderHeaderCell: () => {
      return "Name";
    },
    renderCell: (item) => {
      return item.name;
    },
  }),
  createTableColumn({
    columnId: "lastUpdated",
    compare: (a, b) => {
      return a.lastUpdate.getTime() - b.lastUpdate.getTime();
    },
    renderHeaderCell: () => {
      return "Last updated";
    },
    renderCell: (item) => {
      return item.lastUpdate.toLocaleDateString();
    },
  }),
];

export function SourceList() {
  
  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h3>Data Source</h3>
        <Button>
          Create New
        </Button>
      </div>
      <DataGrid
        items={items}
        columns={columns}
        sortable
        selectionMode="multiselect"
        resizableColumns
        getRowId={(item) => item.id}
      >
        <DataGridHeader>
          <DataGridRow>
            {({ renderHeaderCell }) => (
              <DataGridHeaderCell>
                {renderHeaderCell()}
              </DataGridHeaderCell>
            )}
          </DataGridRow>
        </DataGridHeader>
        <DataGridBody<any>>
          {({ item, rowId }) => (
            <DataGridRow<any> key={rowId}>
              {({ renderCell }) => (
                <DataGridCell>
                  {renderCell(item)}
                </DataGridCell>
              )}
            </DataGridRow>
          )}
        </DataGridBody>
      </DataGrid>
    </div>
  );
}