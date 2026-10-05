import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
import { InterviewService, QuestionEvaluation, TopicEvaluation } from '../../core/services/interview.service';

@Component({
  selector: 'app-topic-evaluation-page',
  standalone: true,
  imports: [CommonModule, WorkspaceLayout, PrimaryButton],
  templateUrl: './topic-evaluation-page.html',
  styleUrl: './interview-flow.scss'
})
export class TopicEvaluationPage implements OnInit {
  readonly interview = inject(InterviewService);
  private readonly router = inject(Router);

  // Read initial topic index from router state or current interview service state
  readonly topicIndex = signal<number>(
    typeof history.state?.topicIndex === 'number'
      ? history.state.topicIndex
      : this.interview.currentTopicIndex()
  );

  readonly session = computed(() => this.interview.session());
  readonly topic = computed(() => {
    const s = this.session();
    const idx = this.topicIndex();
    if (!s || !s.topics || idx < 0 || idx >= s.topics.length) return null;
    return s.topics[idx];
  });

  readonly evaluation = computed<TopicEvaluation | null>(() => {
    const evals = this.interview.evaluations();
    return evals[this.topicIndex()] ?? null;
  });

  readonly sortedQuestionEvaluations = computed<QuestionEvaluation[]>(() => {
    const evalData = this.evaluation();
    if (!evalData || !evalData.questionEvaluations) return [];
    // Ensure original question order
    return [...evalData.questionEvaluations].sort((a, b) => a.questionOrder - b.questionOrder);
  });

  readonly evaluating = computed(() => this.interview.evaluating());
  readonly error = computed(() => this.interview.error());

  readonly hasMoreTopics = computed(() => {
    const s = this.session();
    if (!s || !s.topics) return false;
    return this.topicIndex() + 1 < s.topics.length;
  });

  readonly isFinalTopic = computed(() => !this.hasMoreTopics());

  ngOnInit(): void {
    const idx = this.topicIndex();
    // If this topic evaluation hasn't been fetched yet, initiate it
    if (!this.interview.evaluations()[idx] && !this.interview.evaluating() && this.session()) {
      this.interview.evaluate(idx);
    }
  }

  getQuestionText(questionOrder: number): string {
    const t = this.topic();
    const q = t?.questions?.find(item => item.order === questionOrder);
    return q?.question || `Question ${questionOrder}`;
  }

  getCandidateAnswer(questionOrder: number): string {
    const t = this.topic();
    if (!t) return '';
    const answerRecord = this.interview.answers().find(
      a => a.topicOrder === t.order && a.questionOrder === questionOrder
    );
    return answerRecord?.answer || '';
  }

  getScoreBadgeClass(score: number): string {
    if (score >= 80) return 'score-badge-high';
    if (score >= 60) return 'score-badge-mid';
    return 'score-badge-low';
  }

  retryEvaluation(): void {
    this.interview.evaluate(this.topicIndex());
  }

  selectTopic(index: number): void {
    this.topicIndex.set(index);
    if (!this.interview.evaluations()[index] && !this.interview.evaluating()) {
      this.interview.evaluate(index);
    }
  }

  continueToNextTopic(): void {
    const session = this.session();
    if (!session) return;

    if (this.hasMoreTopics()) {
      const nextIndex = this.topicIndex() + 1;
      this.interview.currentTopicIndex.set(nextIndex);
      this.interview.currentQuestionIndex.set(0);
      this.router.navigate(['/mock-interview/session']);
    } else {
      this.router.navigate(['/mock-interview/report']);
    }
  }

  goToFinalEvaluation(): void {
    this.router.navigate(['/mock-interview/report']);
  }

  goToSetup(): void {
    this.router.navigate(['/mock-interview']);
  }
}
