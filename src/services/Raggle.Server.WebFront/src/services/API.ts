import axios from "axios";

import { Storage } from "./Storage";
import { Assistant, Connection, Knowledge, User } from "@/models/Model";

export class API {

  public static async getUserAsync() {
    return await this.getAsync<User>(`user`);
  }

  public static async getAssistantAsync() {
    return await this.getAsync<Assistant>(`assistant`);
  }

  public static async updateAssistantAsync(assistant: Assistant) {
    return await this.putAsync<Assistant>(`assistant`, assistant);
  }

  public static async getKnowledgesAsync(skip: number, limit: number) {
    return await this.getAsync<Knowledge[]>(`knowledges?skip=${skip}&limit=${limit}`);
  }

  public static async getKnowledgeAsync(id: string) {
    return await this.getAsync<Knowledge>(`knowledge/${id}`);
  }

  public static async createKnowledgeAsync(knowledge: Knowledge) {
    return await this.postAsync<Knowledge>(`knowledge`, knowledge);
  }

  public static async updateKnowledgeAsync(knowledge: Knowledge) {
    return await this.putAsync<Knowledge>(`knowledge`, knowledge);
  }

  public static async deleteKnowledgeAsync(id: string) {
    return await this.deleteAsync(`knowledge/${id}`);
  }

  public static async getConnectionsAsync(skip: number, limit: number) {
    return await this.getAsync<Connection[]>(`connections?skip=${skip}&limit=${limit}`);
  }

  public static async getConnectionAsync(id: string) {
    return await this.getAsync<Connection>(`connection/${id}`);
  }

  public static async createConnectionAsync(connection: Connection) {
    return await this.postAsync<Connection>(`connection`, connection);
  }

  public static async updateConnectionAsync(connection: Connection) {
    return await this.putAsync<Connection>(`connection`, connection);
  }

  public static async deleteConnectionAsync(id: string) {
    return await this.deleteAsync(`connection/${id}`);
  }




  private static async getAsync<T>(url: string) {
    const request = this.createRequest();
    const response = await request.get<T>(url);
    return response.data;
  }

  private static async postAsync<T>(url: string, data: T) {
    const request = this.createRequest();
    const response = await request.post<T>(url, data);
    return response.data;
  }

  private static async putAsync<T>(url: string, data: T) {
    const request = this.createRequest();
    const response = await request.put<T>(url, data);
    return response.data;
  }

  private static async deleteAsync(url: string) {
    const request = this.createRequest();
    const response = await request.delete<boolean>(url);
    return response.data;
  }

  private static createRequest() {
    return axios.create({
      baseURL: `${Storage.host}/api`,
      timeout: 3_000,
      headers: {
        'User-ID': Storage.userId
      }
    });
  }

}