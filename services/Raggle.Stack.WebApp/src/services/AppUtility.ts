import { since } from '@iyulab/hive-stack/internal';

/**
 * 엔티티의 마지막 업데이트 날짜 또는 생성날짜를 현지 시간으로 비교
 */
export const sinceEntity = (entity: any): string => {
  if (!entity) return 'N/A';
  if (entity.lastUpdatedAt)
    return since(entity.lastUpdatedAt);
  if (entity.createdAt)
    return since(entity.createdAt);
  return 'N/A';
}

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
