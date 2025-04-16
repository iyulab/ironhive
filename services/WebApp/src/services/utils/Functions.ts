/**
 * 클라이언트 라우팅을 시도합니다.
 */
export const goTo = (path: string) => {
  window.history.pushState(null, '', path);
  window.dispatchEvent(new PopStateEvent('popstate'));
}

/**
 * 오브젝트를 깊은 복사합니다.
 */
export const clone = (value: any) => {
  return JSON.parse(JSON.stringify(value));
}
