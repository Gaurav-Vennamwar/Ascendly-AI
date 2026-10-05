import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { WorkspaceLayout } from '../../shared/components/workspace-layout/workspace-layout';
import { PrimaryButton } from '../../shared/components/primary-button/primary-button';
import { InterviewService } from '../../core/services/interview.service';

@Component({
  selector: 'app-interview-session-page',
  standalone: true,
  imports: [CommonModule, WorkspaceLayout, PrimaryButton],
  templateUrl: './interview-session-page.html',
  styleUrl: './interview-flow.scss'
})
export class InterviewSessionPage {
  readonly interview = inject(InterviewService);
  private readonly router = inject(Router);

  readonly answer = signal<string>('');

  readonly session = computed(() => this.interview.session());
  readonly topic = computed(() => this.interview.currentTopic());
  readonly question = computed(() => this.interview.currentQuestion());
  readonly submitting = computed(() => this.interview.submitting());
  readonly evaluating = computed(() => this.interview.evaluating());
  readonly error = computed(() => this.interview.error());

  readonly targetRole = computed(() => this.interview.configuration()?.targetRole || 'Target Role');
  readonly interviewerRole = computed(() => this.interview.configuration()?.interviewerRole || 'Technical Interviewer');
  readonly difficulty = computed(() => this.interview.configuration()?.difficulty || 'Intermediate');

  readonly totalQuestions = computed(() => this.interview.totalQuestionsCount());
  readonly questionNumber = computed(() => this.interview.overallQuestionNumber());
  readonly topicQuestionNumber = computed(() => this.interview.currentQuestionIndex() + 1);
  readonly topicTotalQuestions = computed(() => this.topic()?.questions?.length ?? 0);

  readonly isTopicComplete = computed(() => this.interview.isCurrentTopicComplete());

  onAnswerInput(event: Event): void {
    const target = event.target as HTMLTextAreaElement | null;
    if (target) {
      this.answer.set(target.value);
    }
  }

  submitAnswer(): void {
    const currentTopic = this.topic();
    const currentQ = this.question();
    const answerText = this.answer().trim();

    if (!currentTopic || !currentQ || !answerText || this.submitting()) {
      return;
    }

    this.interview.submit(
      {
        topicOrder: currentTopic.order,
        questionOrder: currentQ.order,
        answer: answerText
      },
      () => {
        // Clear answer only after successful persistence
        this.answer.set('');
        // If not the last question of topic, advance to next question
        if (this.interview.currentQuestionIndex() + 1 < (currentTopic.questions?.length ?? 0)) {
          this.interview.currentQuestionIndex.update(idx => idx + 1);
        }
      }
    );
  }

  viewTopicEvaluation(): void {
    const topicIdx = this.interview.currentTopicIndex();
    if (this.evaluating()) return;

    if (this.interview.evaluations()[topicIdx]) {
      this.router.navigate(['/mock-interview/topic-evaluation'], {
        state: { topicIndex: topicIdx }
      });
      return;
    }

    this.interview.evaluate(topicIdx, () => {
      this.router.navigate(['/mock-interview/topic-evaluation'], {
        state: { topicIndex: topicIdx }
      });
    });
  }

  goToSetup(): void {
    this.router.navigate(['/mock-interview']);
  }
}
