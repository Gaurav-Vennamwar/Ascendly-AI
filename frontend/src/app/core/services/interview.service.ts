import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, finalize, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface InterviewQuestion {
  order: number;
  topic: string;
  question: string;
  purpose: string;
}

export interface InterviewTopic {
  name: string;
  order: number;
  purpose: string;
  questions: InterviewQuestion[];
}

export interface StartInterviewResponse {
  sessionId: string;
  topics: InterviewTopic[];
  currentTopicIndex: number;
  currentQuestionIndex: number;
  status: string;
}

export interface InterviewAnswer {
  topicOrder: number;
  questionOrder: number;
  answer: string;
}

export interface SubmitAnswerResponse {
  message: string;
}

export interface QuestionEvaluation {
  questionOrder: number;
  score: number;
  strengths: string[];
  mistakes: string[];
  improvements: string[];
  betterPhrase: string;
}

export interface TopicEvaluation {
  topic: string;
  questionEvaluations: QuestionEvaluation[];
}

export interface FinalInterviewEvaluation {
  overallScore: number;
  communicationScore: number;
  technicalScore: number;
  problemSolvingScore: number;
  behavioralScore: number;
  roleAlignmentScore: number;
  strongestAreas: string[];
  areasToImprove: string[];
  repeatedMistakes: string[];
  preparationPlan: string[];
  interviewReadiness: string;
}

export interface InterviewConfiguration {
  targetRole: string;
  jobDescription: string;
  customTopics: string[];
  interviewType: string;
  difficulty: string;
  durationMinutes: number;
  interviewStyle: string;
  interviewerRole: string;
  interviewerContext: string;
}

interface StoredInterviewState {
  session: StartInterviewResponse | null;
  configuration: InterviewConfiguration | null;
  currentTopicIndex: number;
  currentQuestionIndex: number;
  answers: InterviewAnswer[];
  evaluations: Record<number, TopicEvaluation>;
  finalEvaluation: FinalInterviewEvaluation | null;
}

const STORAGE_KEY = 'ascendly_mock_interview_state';

@Injectable({ providedIn: 'root' })
export class InterviewService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/Interview`;

  readonly session = signal<StartInterviewResponse | null>(null);
  readonly configuration = signal<InterviewConfiguration | null>(null);
  readonly currentTopicIndex = signal<number>(0);
  readonly currentQuestionIndex = signal<number>(0);
  readonly answers = signal<InterviewAnswer[]>([]);
  readonly evaluations = signal<Record<number, TopicEvaluation>>({});
  readonly finalEvaluation = signal<FinalInterviewEvaluation | null>(null);

  readonly starting = signal<boolean>(false);
  readonly submitting = signal<boolean>(false);
  readonly evaluating = signal<boolean>(false);
  readonly completing = signal<boolean>(false);
  readonly error = signal<string>('');

  // Derived state
  readonly currentTopic = computed<InterviewTopic | null>(() => {
    const s = this.session();
    const idx = this.currentTopicIndex();
    if (!s || !s.topics || idx < 0 || idx >= s.topics.length) return null;
    return s.topics[idx];
  });

  readonly currentQuestion = computed<InterviewQuestion | null>(() => {
    const topic = this.currentTopic();
    const idx = this.currentQuestionIndex();
    if (!topic || !topic.questions || idx < 0 || idx >= topic.questions.length) return null;
    return topic.questions[idx];
  });

  readonly totalQuestionsCount = computed<number>(() => {
    const s = this.session();
    if (!s || !s.topics) return 0;
    return s.topics.reduce((sum, topic) => sum + (topic.questions?.length ?? 0), 0);
  });

  readonly overallQuestionNumber = computed<number>(() => {
    const s = this.session();
    if (!s || !s.topics) return 0;
    const currentTopicIdx = this.currentTopicIndex();
    let questionsBeforeCurrent = 0;
    for (let i = 0; i < currentTopicIdx && i < s.topics.length; i++) {
      questionsBeforeCurrent += s.topics[i].questions?.length ?? 0;
    }
    return questionsBeforeCurrent + this.currentQuestionIndex() + 1;
  });

  readonly isCurrentTopicComplete = computed<boolean>(() => {
    const topic = this.currentTopic();
    if (!topic || !topic.questions || topic.questions.length === 0) return false;
    const currentAnswers = this.answers();
    return topic.questions.every(q =>
      currentAnswers.some(a => a.topicOrder === topic.order && a.questionOrder === q.order)
    );
  });

  readonly hasMoreTopics = computed<boolean>(() => {
    const s = this.session();
    if (!s || !s.topics) return false;
    return this.currentTopicIndex() + 1 < s.topics.length;
  });

  constructor() {
    this.restoreState();
  }

  // Raw typed API methods
  startInterview(configuration: InterviewConfiguration, resume: File | null): Observable<StartInterviewResponse> {
    const body = new FormData();
    body.append('TargetRole', configuration.targetRole);
    if (configuration.jobDescription) {
      body.append('JobDescription', configuration.jobDescription);
    }
    body.append('InterviewType', configuration.interviewType);
    body.append('Difficulty', configuration.difficulty);
    body.append('DurationMinutes', String(configuration.durationMinutes));
    body.append('InterviewStyle', configuration.interviewStyle);
    body.append('InterviewerRole', configuration.interviewerRole);
    if (configuration.interviewerContext) {
      body.append('InterviewerContext', configuration.interviewerContext);
    }
    configuration.customTopics.forEach((topic, index) => {
      body.append(`CustomTopics[${index}]`, topic);
    });
    if (resume) {
      body.append('resume', resume, resume.name);
    }

    return this.http.post<StartInterviewResponse>(`${this.url}/start`, body);
  }

  submitAnswer(answer: InterviewAnswer): Observable<SubmitAnswerResponse> {
    const session = this.session();
    if (!session) {
      throw new Error('No active session.');
    }
    return this.http.post<SubmitAnswerResponse>(`${this.url}/${session.sessionId}/answer`, answer);
  }

  evaluateTopic(topicIndex: number): Observable<TopicEvaluation> {
    const session = this.session();
    if (!session) {
      throw new Error('No active session.');
    }
    return this.http.post<TopicEvaluation>(`${this.url}/${session.sessionId}/evaluate-topic/${topicIndex}`, {});
  }

  completeInterview(): Observable<FinalInterviewEvaluation> {
    const session = this.session();
    if (!session) {
      throw new Error('No active session.');
    }
    return this.http.post<FinalInterviewEvaluation>(`${this.url}/${session.sessionId}/complete`, {});
  }

  // Reactive state-driven methods
  start(
    configuration: InterviewConfiguration,
    resume: File | null,
    onSuccess?: () => void,
    onError?: (err: Error) => void
  ): void {
    if (this.starting()) return;

    this.error.set('');
    this.starting.set(true);

    this.startInterview(configuration, resume)
      .pipe(finalize(() => this.starting.set(false)))
      .subscribe({
        next: (response) => {
          this.session.set(response);
          this.configuration.set(configuration);
          this.currentTopicIndex.set(response.currentTopicIndex ?? 0);
          this.currentQuestionIndex.set(response.currentQuestionIndex ?? 0);
          this.answers.set([]);
          this.evaluations.set({});
          this.finalEvaluation.set(null);
          this.persistState();
          onSuccess?.();
        },
        error: (err) => {
          const message = err?.error?.message || err?.error || 'Unable to start the interview. Please try again.';
          this.error.set(typeof message === 'string' ? message : 'Unable to start the interview. Please try again.');
          onError?.(err);
        }
      });
  }

  submit(
    answer: InterviewAnswer,
    onSuccess?: () => void,
    onError?: (err: Error) => void
  ): void {
    const session = this.session();
    if (!session || this.submitting()) return;

    this.error.set('');
    this.submitting.set(true);

    this.submitAnswer(answer)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          // Add answer safely, matching both topicOrder and questionOrder
          this.answers.update(items => [
            ...items.filter(item => !(item.topicOrder === answer.topicOrder && item.questionOrder === answer.questionOrder)),
            answer
          ]);
          this.persistState();
          onSuccess?.();
        },
        error: (err) => {
          const message = err?.error?.message || err?.error || 'Your answer was not saved. Please try again; your text is still here.';
          this.error.set(typeof message === 'string' ? message : 'Your answer was not saved. Please try again; your text is still here.');
          onError?.(err);
        }
      });
  }

  evaluate(
    topicIndex: number,
    onSuccess?: (result: TopicEvaluation) => void,
    onError?: (err: Error) => void
  ): void {
    const session = this.session();
    if (!session || this.evaluating()) return;

    // Do not call endpoint more than once for the same topic
    const existing = this.evaluations()[topicIndex];
    if (existing) {
      onSuccess?.(existing);
      return;
    }

    this.error.set('');
    this.evaluating.set(true);

    this.evaluateTopic(topicIndex)
      .pipe(finalize(() => this.evaluating.set(false)))
      .subscribe({
        next: (result) => {
          this.evaluations.update(evals => ({
            ...evals,
            [topicIndex]: result
          }));
          this.persistState();
          onSuccess?.(result);
        },
        error: (err) => {
          const message = err?.error?.message || err?.error || 'We could not prepare this topic evaluation. Please try again.';
          this.error.set(typeof message === 'string' ? message : 'We could not prepare this topic evaluation. Please try again.');
          onError?.(err);
        }
      });
  }

  complete(
    onSuccess?: (result: FinalInterviewEvaluation) => void,
    onError?: (err: Error) => void
  ): void {
    const session = this.session();
    if (!session || this.completing()) return;

    const existing = this.finalEvaluation();
    if (existing) {
      onSuccess?.(existing);
      return;
    }

    this.error.set('');
    this.completing.set(true);

    this.completeInterview()
      .pipe(finalize(() => this.completing.set(false)))
      .subscribe({
        next: (result) => {
          this.finalEvaluation.set(result);
          this.persistState();
          onSuccess?.(result);
        },
        error: (err) => {
          const message = err?.error?.message || err?.error || 'We could not generate your final evaluation. Please try again.';
          this.error.set(typeof message === 'string' ? message : 'We could not generate your final evaluation. Please try again.');
          onError?.(err);
        }
      });
  }

  reset(): void {
    this.session.set(null);
    this.configuration.set(null);
    this.currentTopicIndex.set(0);
    this.currentQuestionIndex.set(0);
    this.answers.set([]);
    this.evaluations.set({});
    this.finalEvaluation.set(null);
    this.error.set('');
    try {
      sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      // Ignore storage errors
    }
  }

  private persistState(): void {
    try {
      const state: StoredInterviewState = {
        session: this.session(),
        configuration: this.configuration(),
        currentTopicIndex: this.currentTopicIndex(),
        currentQuestionIndex: this.currentQuestionIndex(),
        answers: this.answers(),
        evaluations: this.evaluations(),
        finalEvaluation: this.finalEvaluation()
      };
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
      // Storage might be full or disabled, gracefully degrade
    }
  }

  private restoreState(): void {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return;
      const state = JSON.parse(raw) as StoredInterviewState;
      if (state && state.session) {
        this.session.set(state.session);
        this.configuration.set(state.configuration ?? null);
        this.currentTopicIndex.set(state.currentTopicIndex ?? 0);
        this.currentQuestionIndex.set(state.currentQuestionIndex ?? 0);
        this.answers.set(state.answers ?? []);
        this.evaluations.set(state.evaluations ?? {});
        this.finalEvaluation.set(state.finalEvaluation ?? null);
      }
    } catch {
      // Ignore corrupt session storage
    }
  }
}
