export type SourceType = "file" | "openapi" | "sqlserver" | "postgres"

export interface DataSource {
  $type: SourceType,
  id: string,
  name?: string,
  description?: string,
  createdAt?: Date,
  updatedAt?: Date,
}

export interface FileSource extends DataSource {
  files?: FileMeta[]
}

export interface SqlServerSource extends DataSource {
  connectionString?: string
}

export interface PostgresSource extends DataSource {
  connectionString?: string
}

export interface OpenApiSource extends DataSource {
  url?: string
}

export interface FileMeta extends DataSource {
  name: string,
  mimeType: string,
  size: number,
  path: string,
}
