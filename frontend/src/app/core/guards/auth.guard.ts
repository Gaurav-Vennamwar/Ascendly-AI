import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check whether an access token exists.
  const token = authService.getAccessToken();

  if (token) {
    // User has an access token, so allow navigation.
    return true;
  }

  // No token → user is not authenticated.
  // Send them back to the login page.
  return router.createUrlTree(['/login']);
};
