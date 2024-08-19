import { API } from "./API";
import { Storage } from "./Storage";
import { Assistant, User } from "@/models/Model";

export class App {
  public static user: User;
  public static assistant: Assistant;

  public static async initAsync() {
    this.user = await API.getUserAsync();
    Storage.userId = this.user.id;
    this.assistant = await API.getAssistantAsync();
  }
  
}