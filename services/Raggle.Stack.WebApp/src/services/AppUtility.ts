/** 
 * 문자열 날짜를 현지 시간으로 비교하여 시간 경과를 표시합니다.
 */
export const since = (value?: string): string => {
  if (!value) return 'N/A';

  const date = new Date(value); // 입력 값 (UTC 시간)
  const now = new Date(); // 현재 클라이언트 로컬 시간
  // 클라이언트의 UTC 오프셋 (분 단위)
  const offset = value.endsWith('Z') ? 0 : now.getTimezoneOffset() * 60 * 1000;

  // UTC 기준 시간 차이 계산
  const seconds = Math.floor((now.getTime() - (date.getTime() - offset)) / 1000);

  if (seconds < 0) return `N/A`;
  if (seconds < 60) return `${seconds} seconds ago`;
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes} minutes ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} hours ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days} days ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months} months ago`;
  const years = Math.floor(months / 12);
  return `${years} years ago`;
}

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
 * 문자열 날짜를 현지 시간으로 기본 포맷
 */
export const formatDate = (value?: string): string => {
  if (!value) return 'N/A';
  let date = new Date(value);
  const offset = value.endsWith('Z') ? 0 : date.getTimezoneOffset() * 60 * 1000;
  date = new Date(date.getTime() - offset);
  const formatter = new Intl.DateTimeFormat('ko-KR', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
  return formatter.format(date);
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
