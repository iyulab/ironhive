export type SourceType = "file" | "openapi" | "sqlserver" | "mongo"

export interface DataSource {
  type: SourceType,
  id: string,
  userID: string
  name?: string,
  description?: string,
  details?: any,
  createdAt?: Date,
  updatedAt?: Date,
}

export interface FileDetails {
  files?: FileMeta[]
}

export interface DatabaseDetails {
  connectionString?: string
}

export interface OpenApiDetails {
  schema?: string
}

export interface FileMeta {
  type: string,
  name: string,
  size: number
}

export type SourceDisplay = {
  type: SourceType;
  label: string;
  img: string;
}

export const sourcesList: SourceDisplay[] = [
  { type: "file", label: "File", img: "" },
  { type: "openapi", label: "Open API", img: "" },
  { type: "sqlserver", label: "Microsoft SQL Server", img: "" },
  { type: "mongo", label: "Mongo DB", img: "" },
]