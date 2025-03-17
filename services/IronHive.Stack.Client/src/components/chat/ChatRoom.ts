import { LitElement, PropertyValues, css, html, nothing } from "lit";
import { customElement, property, query } from "lit/decorators.js";

import type { Message, MessageContent } from "../../models";
import { HiveStack } from "../../services";
import { SubmitMessageEvent, StopMessageEvent } from "../events";
import { HttpResponse } from "../../internal";
import { MessageList } from "./MessageList";

@customElement('chat-room')
export class ChatRoom extends LitElement {
  private _client?: HiveStack;
  private _res?: HttpResponse;

  @query('message-list') messageEl!: MessageList;

  @property({ type: String }) baseUri: string = '';
  @property({ type: Boolean }) loading: boolean = false;
  @property({ type: Array }) messages: Message[] = [
//     {
//       role: 'assistant',
//       content: [
//         { type: 'text', value: 'Hello! I am a chatbot. How can I help you today? 😊' }
//       ],
//       timestamp: new Date().toISOString()
//     },
//     {
//       role: 'assistant',
//       content: [
//         { type: 'text', value: 'I can help you with various tasks, \n like finding information, answering questions, and more.' }
//       ],
//       timestamp: new Date().toISOString()
//     },
//     {
//       role: 'assistant',
//       content: [
//         { type: 'text', value: 'Feel free to ask me anything!' }
//       ],
//       timestamp: new Date().toISOString()
//     },
//     {
//       role: 'assistant',
//       content: [
//         { type: 'text', value: `옛날 옛적, 평화로운 마을이 한가득 펼쳐진 푸른 숲 속에 자리 잡고 있었습니다.
// 이 마을에는 호기심 많고 용감한 소년 민준이 살고 있었으며, 그는 언제나 새로운 모험을 꿈꾸었습니다.
// 어느 날, 마을 위에 갑자기 어두운 그림자가 드리워지며, 신비로운 소문이 퍼지기 시작했습니다.
// 사람들은 숲 깊은 곳에 잠든 고대 마법의 힘이 깨어날 조짐을 느끼며 불안에 떨었습니다.
// 민준은 마을 어른들의 이야기를 듣고, 자신이 그 비밀을 밝혀내야 한다고 굳게 결심했습니다.
// 이른 아침, 민준은 필요한 물건들을 챙기고 두근거리는 마음으로 모험을 향해 첫 발을 내디뎠습니다.
// 숲의 나무들은 마치 살아있는 듯 민준을 반겨주었고, 바람은 조용히 속삭이며 길을 안내해 주었습니다.
// 걷는 도중, 민준은 반짝이는 날개를 가진 작은 요정 수아를 만나게 되었고, 두 친구는 곧 뜻을 같이하게 되었습니다.
// 수아는 민준에게 오래된 전설과 마법의 지팡이가 숨겨진 신비로운 동굴의 위치를 알려 주었습니다.
// 두 친구는 함께 힘을 합쳐, 전설 속 동굴을 찾아 깊은 숲속을 헤매기 시작했습니다.
// 긴 여정 끝에, 그들은 반짝이는 빛이 새어 나오는 동굴 입구에 마주하게 되었고, 긴장과 기대가 뒤섞였습니다.
// 동굴 안으로 들어서자, 벽면에 그려진 고대의 기록과 신비로운 문양들이 민준의 호기심을 자극했습니다.
// 그러던 중, 동굴 깊숙한 곳에서 낮고 거친 목소리가 울려 퍼지며, 숨겨진 마법의 수호자가 모습을 드러냈습니다.
// 수호자는 민준에게 고대 마법의 힘이 마을과 숲을 위협하고 있다는 경고를 전하며, 이를 막기 위한 비밀 주문을 전수했습니다.
// 민준과 수아는 두려움 속에서도 그 주문을 연습하며, 동굴 곳곳에서 나타나는 여러 시련을 하나씩 극복해 나갔습니다.
// 그러던 중, 어둠의 기운을 품은 마법사와의 치열한 대결이 시작되었고, 민준은 용기와 지혜를 총동원해 맞섰습니다.
// 마법의 지팡이와 고대 주문의 힘을 빌린 민준은, 수아와 함께 어둠의 마법사의 공격을 물리치기 위해 최선을 다했습니다.
// 격렬한 전투 끝에, 어둠의 마법사는 자신의 오만함을 깨닫고, 마침내 숲과 마을에 평화를 되찾겠다는 결심을 하게 되었습니다.
// 마법의 수호자와 함께 어둠의 마법사는 고대의 질서를 바로 세우며, 숲 속 모든 생명에게 다시 평화를 약속하였습니다.
// 모험을 마치고 돌아온 민준은 마을 사람들에게 자신이 겪은 신비로운 이야기를 들려주며, 진정한 용기와 희망은 누구나 마음 속 깊은 곳에서 비롯된다는 귀중한 교훈을 남겼습니다.` }
//       ],
//       timestamp: new Date().toISOString()
//     }
  ];

  protected async updated(_changedProperties: PropertyValues) {
    super.updated(_changedProperties);
    if (_changedProperties.has('baseUri')) {
      this._client = new HiveStack({ baseUrl: this.baseUri });
    }
  }

  render() {
    return html`
      <div class="container">
        <message-list
          .messages=${this.messages}
        ></message-list>
        
        <message-input
          placeholder="Type a message..."
          @submit=${this.handleSubmit}
          @stop=${this.handleStop}>
        ></message-input>
      </div>
    `;
  }

  private handleStop = (e: StopMessageEvent) => {
    this._res?.cancel();
  }

  private handleSubmit = async (e: SubmitMessageEvent) => {
    const value = e.detail;
    const user_msg: Message = {
      role: 'user',
      content: [{ type: 'text', value: value }],
      timestamp: new Date().toISOString()
    }
    const bot_msg: Message = {
      role: 'assistant',
      content: [],
      timestamp: new Date().toISOString()
    }
    this.messages = [...this.messages, user_msg];
    const anth = "anthropic/claude-3-5-haiku-latest";
    const open = "openai/gpt-4o-mini";

    this._res = await this._client?.chatCompletionAsync({
      model: open,
      messages: this.messages,
      system: "you are a chatbot politely responding to user messages",
      stream: true
    }, (item) => {
      let last = this.messages[this.messages.length - 1];
      if (last.role !== 'assistant') {
        this.messages = [...this.messages, bot_msg];
        last = this.messages[this.messages.length - 1];
      }

      if(item.endReason) {
        console.log("End Reason: ", item.endReason);
      } else if(item) {
        const data = item as MessageContent;
        const index = data.index || 0;
        last.content ||= [];
        const content = last.content?.at(index);
        console.log("Data: ", data);

        if (content) {
          if (content.type === 'text' && data.type === 'text') {
            content.value ||= '';
            content.value += data.value;
          } else {
            last.content[index] = data;
          }
        } else {
          last.content?.push(data);
        }
      }
      this.messageEl.requestUpdate();
      // this.messageEl.scrollTo({
      //   top: this.messageEl.scrollHeight,
      //   behavior: 'instant'
      // });
    });
  }

  static styles = css`
    .container {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      height: 100%;
      min-width: 320px;
      min-height: 480px;
      color: var(--hs-text-color);
      background-color: var(--hs-background-color);
      overflow: hidden;
    }
  `;
}
