export const since = (value?: string): string => {
  if (!value) return 'N/A';
  console.log('Input UTC date:', value);

  const date = new Date(value); // 입력 값 (UTC 시간)
  const now = new Date(); // 현재 클라이언트 로컬 시간

  // 클라이언트의 UTC 오프셋 (분 단위)
  const offset = now.getTimezoneOffset() * 60 * 1000;

  // UTC 기준 시간 차이 계산
  const seconds = Math.floor((now.getTime() - (date.getTime() - offset)) / 1000);

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

export const sinceEntity = (entity: any): string => {
  if (!entity) return 'N/A';
  if (entity.createdAt)
    return since(entity.createdAt);
  if (entity.lastUpdatedAt)
    return since(entity.lastUpdatedAt);
  return 'N/A';
}

export const goTo = (path: string) => {
  window.history.pushState(null, '', path);
  window.dispatchEvent(new PopStateEvent('popstate'));
}
