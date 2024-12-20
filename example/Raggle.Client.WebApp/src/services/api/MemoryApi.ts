import { HttpController } from '../http/HttpController';
import { CollectionEntity, DocumentEntity } from '../../models';
import { HttpResponse } from '../http/HttpResponse';

export class MemoryApi {
  constructor(private controller: HttpController) {}

  public async findCollections(
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<CollectionEntity[]> {
    const res = await this.controller.get('/memory', { 
      params: { limit, skip, order, name: name ?? '' }
    });
    return await res.json<CollectionEntity[]>();
  }

  public async getCollection(
    collectionId: string
  ): Promise<CollectionEntity> {
    const res = await this.controller.get(`/memory/${collectionId}`);
    return await res.json<CollectionEntity>();
  }

  public async upsertCollection(
    entity: CollectionEntity
  ): Promise<CollectionEntity> {
    const res = await this.controller.post('/memory', entity);
    return await res.json<CollectionEntity>();
  }

  public async deleteCollection(
    collectionId: string
  ): Promise<void> {
    await this.controller.delete(`/memory/${collectionId}`);
  }

  public async searchCollection(
    collectionId: string, 
    query: string
  ): Promise<any> {
    const res = await this.controller.post(`/memory/${collectionId}/search`, {
      query,
    });
    return res;
  }

  public async findDocuments(
    collectionId: string,
    name?: string,
    limit: number = 10,
    skip: number = 0,
    order: string = 'desc'
  ): Promise<DocumentEntity[]> {
    const res = await this.controller.get(`/memory/${collectionId}/documents`,{ 
      params: { limit, skip, order, name: name ?? '' }
    });
    return await res.json<DocumentEntity[]>();
  }

  public async uploadDocument(
    collectionId: string, 
    files: File[], 
    tags: string[] = [],
    onProgress?: (progress: number) => void
  ): Promise<HttpResponse> {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });
    tags.forEach((tag) => {
      formData.append('tags', tag);
    });
    return await this.controller.post(`/memory/${collectionId}/documents`, formData, {
      onProgress: onProgress
    });
  }

  public async deleteDocument(
    collectionId: string, 
    documentId: string
  ): Promise<void> {
    await this.controller.delete(`/memory/${collectionId}/documents/${documentId}`);
  }
  
}