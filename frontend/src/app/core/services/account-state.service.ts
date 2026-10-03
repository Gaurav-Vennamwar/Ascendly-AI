import { Injectable, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { AuthService, CurrentUserResponse } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AccountStateService {
  private readonly auth = inject(AuthService);
  readonly user = signal<CurrentUserResponse | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');

  load(): void {
    if (this.user() || this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    this.auth.getCurrentUser().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: (user) => this.user.set(user),
      error: () => this.error.set('Unable to load your account details right now.')
    });
  }

  clear(): void { this.user.set(null); this.error.set(''); }
}
