/**
 * AbortToken 클래스는 작업 취소를 관리합니다.
 * @class
 */
export class AbortToken {
  private readonly _controller: AbortController = new AbortController();
  private _isAborted = false;
  private _callback?: () => void;

  /**
   * Signature: AbortToken
   * @returns {AbortController.signal} AbortSignal 인스턴스
   */
  public get signal(): AbortSignal {
    return this._controller.signal;
  }

  /**
   * 작업이 취소되었는지 확인하는 getter
   * @returns {boolean} 취소 여부
   */
  public get isAborted(): boolean {
    return this._isAborted;
  }

  /**
   * 취소 콜백을 등록하는 메서드
   * @param {() => void} callback 취소 시 실행될 콜백 함수
   */
  public register(callback: () => void): void {
    this._callback = callback;
  }

  /**
   * 작업을 취소하고, 등록된 콜백을 실행하는 메서드
   * @returns {void}
   */
  public cancel(): void {
    if (!this._isAborted) {
      this._isAborted = true;
      this._controller.abort();
      if (this._callback) {
        this._callback();
      }
    }
  }
}
