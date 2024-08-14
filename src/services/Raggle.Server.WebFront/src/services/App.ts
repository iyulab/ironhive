import { API } from "./API";
import { Assistant, User } from "@/models/Model";

export class App {
  public static user: User;
  public static assistant: Assistant;

  public static async initAsync() {
    this.user = await API.getUserAsync();
    this.assistant = await API.getAssistantAsync();
  }
  
}