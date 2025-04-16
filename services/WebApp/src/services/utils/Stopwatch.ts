/**
 * 스탑워치 클래스
 */
export class Stopwatch {
  private startTime: number = 0;
  private elapsedTime: number = 0;
  private timerInterval: NodeJS.Timeout | null = null;
  private laps: string[] = [];

  /**
   * 시간을 "MM:SS:MS" 형식의 문자열로 변환하는 메서드
   * @param time 밀리초 단위의 시간
   * @returns 형식화된 시간 문자열
   */
  private timeToString(time: number): string {
    const diffInHrs = time / 3600000;
    const hh = Math.floor(diffInHrs);

    const diffInMin = (diffInHrs - hh) * 60;
    const mm = Math.floor(diffInMin);

    const diffInSec = (diffInMin - mm) * 60;
    const ss = Math.floor(diffInSec);

    const diffInMs = (diffInSec - ss) * 100;
    const ms = Math.floor(diffInMs);

    const formattedMM = mm.toString().padStart(2, '0');
    const formattedSS = ss.toString().padStart(2, '0');
    const formattedMS = ms.toString().padStart(2, '0');

    return `${formattedMM}:${formattedSS}:${formattedMS}`;
  }

  /**
   * 현재 경과 시간을 형식화하여 반환하는 메서드
   * @returns 형식화된 경과 시간 문자열
   */
  public get elapsed(): string {
    return this.timeToString(this.elapsedTime);
  }

  /**
   * 스탑워치를 시작하거나 재개하는 메서드
   */
  public start(): void {
    if (this.timerInterval) return; // 이미 타이머가 작동 중이면 무시

    this.startTime = Date.now() - this.elapsedTime;
    this.timerInterval = setInterval(() => {
      this.elapsedTime = Date.now() - this.startTime;
    }, 10);
  }

  /**
   * 스탑워치를 정지하는 메서드
   */
  public stop(): void {
    if (!this.timerInterval) return; // 타이머가 작동 중이지 않으면 무시

    clearInterval(this.timerInterval);
    this.timerInterval = null;
  }

  /**
   * 스탑워치를 초기화하는 메서드
   */
  public reset(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    this.startTime = 0;
    this.elapsedTime = 0;
    this.laps = [];
    console.log("00:00:00");
  }

  /**
   * 현재 경과 시간을 랩 타임으로 기록하는 메서드
   * @returns 기록된 랩 타임 문자열 또는 null
   */
  public lap(): string | null {
    if (this.timerInterval) {
      const lapTime = this.timeToString(this.elapsedTime);
      this.laps.push(lapTime);
      console.log(`Lap ${this.laps.length}: ${lapTime}`);
      return lapTime;
    } else {
      console.log("스탑워치가 작동 중이지 않습니다.");
      return null;
    }
  }

  /**
   * 기록된 모든 랩 타임을 배열로 반환하는 메서드
   * @returns 랩 타임 배열
   */
  public getLaps(): string[] {
    return [...this.laps]; // 배열 복사본 반환
  }
}
